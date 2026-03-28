using Azure;
using Azure.Identity;
using Datalayer.Interfaces;
using Infastructure.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using Shared.ExternalModels;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using static System.Net.WebRequestMethods;
using DriveInfo = Shared.ExternalModels.DriveInfo;

namespace Infastructure.Implementation
{
    public class MicrosoftOperations : IMicrosoftOperations
    {
        private readonly ILogger<MicrosoftOperations> _logger;
        private readonly IOptions<ADKeyValues> _config;
        private readonly IOperationManager _opsMan;

        public MicrosoftOperations(ILogger<MicrosoftOperations> logger, IOptions<ADKeyValues> config, IOperationManager opsMan)
        {
            _logger = logger;
            _opsMan = opsMan;
            _config = config;
        }

        public async Task<List<DriveInfo>> DiscoverSharePointSites(string tenantId, string clientId, string clientSecret)
        {
            var graphClient = GetGraphClientForTenant(tenantId, clientId, clientSecret);
            //var sites = new List<SiteInfo>();
            
            try
            {
                var drives = await graphClient.Drives.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Select = new[]
                    {
                    "id",
                    "name",
                    "webUrl",
                    "driveType",
                    "createdDateTime",
                    "owner",
                    "sharepointIds"
                };
                    requestConfiguration.QueryParameters.Top = 100;
                });

                // Filter for SharePoint document libraries
                return drives?.Value?
                    .Where(d => d.DriveType == "documentLibrary" && d.WebUrl != null)
                    .Select(d => new DriveInfo
                    {
                        Id = d.Id,
                        Name = d.Name,
                        WebUrl = d.WebUrl,
                        DriveType = d.DriveType,
                        Created = d.CreatedDateTime,
                        SiteId = d.SharePointIds.SiteId
                        //SiteUrl = GetSiteUrlFromDriveUrl(d.WebUrl)
                    })
                    .ToList() ?? new List<DriveInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error discovering sites: {ex.Message}");
                return new List<DriveInfo>();
            }
        }

        private async Task<bool> IsSharePointAvailable(string tenantId, string clientId, string clientSecret)
        {
            var graphClient = GetGraphClientForTenant(tenantId, clientId, clientSecret);

            try
            {
                // Try a different endpoint first
                var drives = await graphClient.Drives.GetAsync();
                return drives.Value != null && drives.Value.Any();
            }
            catch
            {
                try
                {
                    // Try to get the organization details
                    var organization = await graphClient.Organization.GetAsync();
                    var domain = organization.Value?.FirstOrDefault()?.VerifiedDomains?.FirstOrDefault();

                    Console.WriteLine($"Tenant domain: {domain?.Name}");
                    Console.WriteLine($"SharePoint URL should be: https://{domain?.Name}.sharepoint.com/");

                    // SharePoint Online URLs follow this pattern
                    // If the domain doesn't match, SharePoint might not be provisioned

                    return !string.IsNullOrEmpty(domain?.Name);
                }
                catch
                {
                    return false;
                }
            }
        }

        private GraphServiceClient GetGraphClientForTenant(string tenantId, string clientId, string clientSecret)
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            var scopes = new[] { "https://graph.microsoft.com/.default" };

            return new GraphServiceClient(credential, scopes);
        }

        public async Task<string> GetOrganizationDomain(string accessToken)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync("https://graph.microsoft.com/v1.0/organization");
                var content = await response.Content.ReadAsStringAsync();

                var json = JsonConvert.DeserializeObject<OrganizationResponse>(content);

                var domain = json.value[0].verifiedDomains.First(d => d.isDefault == true).name;

                return domain;
            }
            catch (Exception ex) { return String.Empty; }
        }

        private async Task<string> GetAccessToken(string tenantId)
        {
            var client = new HttpClient();
            try
            {
                var body = new Dictionary<string, string>
                    {
                        { "grant_type", "client_credentials" },
                        { "client_id", _config.Value.ClientId },
                        { "client_secret", _config.Value.ClientSecret },
                        { "resource", "https://marketplaceapi.microsoft.com" }
                    };

                var response = await client.PostAsync(
                    $"https://login.microsoftonline.com/{tenantId}/oauth2/token",
                    new FormUrlEncodedContent(body));

                var json = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(json);

                return result.access_token;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Exception at GetAccessToken ||| {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }

        public async Task<MarketplaceSubscription> ResolveAsync(string token, string tenantId)
        {
            try
            {
                var accessToken = await GetAccessToken(tenantId);

                if(accessToken == null)
                {
                    return null;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.PostAsync($"https://marketplaceapi.microsoft.com/api/saas/subscriptions/resolve?api-version=2018-08-31", new StringContent(
                    JsonConvert.SerializeObject(new { token }), Encoding.UTF8, "application/json"));

                _logger.LogInformation($"Raw Response from ResolveAsync ||| {JsonConvert.SerializeObject(response)}");

                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Response from ResolveAsync ||| {content}");

                return JsonConvert.DeserializeObject<MarketplaceSubscription>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at ResolveAsync ||| {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }

        public async Task ActivateAsync(string subscriptionId, string tenantId)
        {
            try
            {
                var accessToken = await GetAccessToken(tenantId);

                if (accessToken == null)
                {
                    _logger.LogInformation($"Raw Response from GetAccessToken ||| {accessToken}");
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync($"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}/activate?api-version=2018-08-31");

                _logger.LogInformation($"Raw Response from ActivateAsync ||| {JsonConvert.SerializeObject(response)}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at ActivateAsync ||| {JsonConvert.SerializeObject(ex)}");
            }
        }

        public async Task<ResponseHandler> CancelSubscription(string subscriptionId, string tenantId)
        {
            try
            {
                var accessToken = await GetAccessToken(tenantId);

                if (accessToken == null)
                {
                    _logger.LogInformation($"Raw Response from GetAccessToken ||| {accessToken}");
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.DeleteAsync($"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}?api-version=2018-08-31");

                _logger.LogInformation($"Raw Response from CancelSubscription ||| {JsonConvert.SerializeObject(response)}");

                return new ResponseHandler
                {
                    StatusCode = (int)response.StatusCode,
                    Message = response.ReasonPhrase
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at CancelSubscription ||| {JsonConvert.SerializeObject(ex)}");

                return new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An Error Occured"
                };
            }
        }
        public async Task<CIMarketplaceSubscription> GetSubscription(string subscriptionId, string tenantId)
        {
            try
            {
                var accessToken = await GetAccessToken(tenantId);

                if (accessToken == null)
                {
                    return null;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync($"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}");

                _logger.LogInformation($"Raw Response from GetSubscription ||| {JsonConvert.SerializeObject(response)}");

                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Response from GetSubscription ||| {content}");

                return JsonConvert.DeserializeObject<CIMarketplaceSubscription>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at GetSubscription ||| {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }
    }
}
