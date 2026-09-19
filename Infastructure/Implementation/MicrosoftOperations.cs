using Azure.Identity;
using Datalayer.Interfaces;
using Infastructure.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using Shared.ExternalModels;
using Shared.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using DriveInfo = Shared.ExternalModels.DriveInfo;

namespace Infastructure.Implementation
{
    public class MicrosoftOperations : IMicrosoftOperations
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memCache;
        private readonly IMemoryCacheManager _memCacheManager;
        private readonly ILogger<MicrosoftOperations> _logger;
        private readonly IOptions<ADKeyValues> _config;
        private readonly IOperationManager _opsMan;

        public MicrosoftOperations(ILogger<MicrosoftOperations> logger, IOptions<ADKeyValues> config, IOperationManager opsMan, IHttpClientFactory httpClientFactory, IMemoryCacheManager memCacheManager, IMemoryCache memCache)
        {
            _logger = logger;
            _opsMan = opsMan;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _memCacheManager = memCacheManager;
            _memCache = memCache;
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
                var client = _httpClientFactory.CreateClient();
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
            try
            {
                if (!_memCache.TryGetValue($"AccessToken-{tenantId}", out string accessToken))
                {
                    var client = _httpClientFactory.CreateClient();

                    var body = new Dictionary<string, string>
                        {
                            { "grant_type", "client_credentials" },
                            { "client_id", _config.Value.ClientId },
                            { "client_secret", _config.Value.ClientSecret },
                            { "resource", _config.Value.MarketplaceResource }
                        };

                    var response = await client.PostAsync(
                        $"https://login.microsoftonline.com/{tenantId}/oauth2/token",
                        new FormUrlEncodedContent(body));

                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response from GetAccessToken ||| {json}");
                    dynamic result = JsonConvert.DeserializeObject(json);

                    await _memCacheManager.SetCache($"AccessToken-{tenantId}", result.access_token);

                    return result.access_token;
                }

                return await Task.FromResult(accessToken);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Exception at GetAccessToken ||| {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }

        //public async Task<ResolveTokenResponse> ResolveAsync(string token, string tenantId)
        //{
        //    try
        //    {
        //        var accessToken = await GetAccessToken(tenantId);

        //        if(accessToken == null)
        //        {
        //            return null;
        //        }

        //        var client = _httpClientFactory.CreateClient();
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //        client.DefaultRequestHeaders.Add("x-ms-marketplace-token", Uri.UnescapeDataString(token));


        //        var response = await client.PostAsync($"https://marketplaceapi.microsoft.com/api/saas/subscriptions/resolve?api-version=2018-08-31", null);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var err = await response.Content.ReadAsStringAsync();
        //            _logger.LogError($"Marketplace API failed: {response.StatusCode} ||| {err}");
        //            return null; // or throw
        //        }

        //        _logger.LogInformation($"Raw Response from ResolveAsync ||| {JsonConvert.SerializeObject(response)}");

        //        var content = await response.Content.ReadAsStringAsync();

        //        _logger.LogInformation($"Response from ResolveAsync ||| {content}");

        //        return JsonConvert.DeserializeObject<ResolveTokenResponse>(content);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Exception at ResolveAsync ||| {JsonConvert.SerializeObject(ex)}");
        //        return null;
        //    }
        //}

        //public async Task ActivateAsync(string subscriptionId, string tenantId)
        //{
        //    try
        //    {
        //        var accessToken = await GetAccessToken(tenantId);

        //        if (accessToken == null)
        //        {
        //            _logger.LogInformation($"Raw Response from GetAccessToken ||| {accessToken}");
        //            return;
        //        }

        //        var client = _httpClientFactory.CreateClient();
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //        var response = await client.PostAsync($"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}/activate?api-version=2018-08-31", null);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var err = await response.Content.ReadAsStringAsync();
        //            _logger.LogError($"Marketplace API failed: {response.StatusCode} ||| {err}");
        //            return; // or throw
        //        }

        //        _logger.LogInformation($"Raw Response from ActivateAsync ||| {JsonConvert.SerializeObject(response)}");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Exception at ActivateAsync ||| {JsonConvert.SerializeObject(ex)}");
        //    }
        //}

        public async Task<CIMarketplaceSubscription> ResolveAsync(string token, string tenantId)
        {
            var accessToken = await GetAccessToken(tenantId);
            if (accessToken == null) return null;

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Add("x-ms-marketplace-token", Uri.UnescapeDataString(token));

            var response = await client.PostAsync("https://marketplaceapi.microsoft.com/api/saas/subscriptions/resolve?api-version=2018-08-31", null);

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"ResolveAsync {response.StatusCode} ||| {content}");

            if (!response.IsSuccessStatusCode) return null;

            return JsonConvert.DeserializeObject<CIMarketplaceSubscription>(content);
        }

        public async Task<bool> ActivateAsync(string subscriptionId, string tenantId)
        {
            var accessToken = await GetAccessToken(tenantId);
            if (accessToken == null)
            {
                _logger.LogError("ActivateAsync: no access token.");
                return false;
            }

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Add("x-ms-marketplace-token", Uri.UnescapeDataString(accessToken));

            var response = await client.PostAsync($"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}/activate?api-version=2018-08-31", null);

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"ActivateAsync {response.StatusCode} ||| {body}");

            return response.IsSuccessStatusCode;
        }

        public async Task<ResponseHandler> CancelSubscription(string subscriptionId, string tenantId)
        {
            try
            {
                var accessToken = await GetAccessToken(tenantId);

                if (accessToken == null)
                {
                    _logger.LogInformation($"Raw Response from GetAccessToken ||| {accessToken}");
                    return new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized,
                        Message = "Could not obtain access token"
                    };
                }

                var client =  _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.DeleteAsync($"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}?api-version=2018-08-31");

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"GetSubscription failed: {response.StatusCode} ||| {err}");
                    return null;
                }

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
                    _logger.LogInformation($"Raw Response from GetAccessToken ||| {accessToken}");
                    return null;
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync($"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}?api-version=2018-08-31");

                _logger.LogInformation($"Raw Response from GetSubscription ||| {JsonConvert.SerializeObject(response)}");

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"GetSubscription failed: {response.StatusCode} ||| {err}");
                    return null;
                }

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

        public async Task<ResponseHandler> ChangeQuantity(string subscriptionId, int newQuantity, string tenantId)
        {
            try
            {
                var accessToken = await GetAccessToken(tenantId);
                if (accessToken == null)
                {
                    _logger.LogError("ChangeQuantity: no access token.");
                    return new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized,
                        Message = "Could not obtain access token"
                    };
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var payload = new { quantity = newQuantity };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}?api-version=2018-08-31", content);

                var body = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"ChangeQuantity {response.StatusCode} ||| {body}");

                return new ResponseHandler
                {
                    StatusCode = (int)response.StatusCode,
                    Message = response.IsSuccessStatusCode ? "Quantity change submitted" : body
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at ChangeQuantity ||| {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occurred"
                };
            }
        }
    }
}
