using Azure.Identity;
using Datalayer.Interfaces;
using Infastructure.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Shared.ExternalModels;
using DriveInfo = Shared.ExternalModels.DriveInfo;

namespace Infastructure.Implementation
{
    public class MicrosoftOperations : IMicrosoftOperations
    {
        private readonly ILogger<MicrosoftOperations> _logger;
        private readonly IOperationManager _opsMan;

        public MicrosoftOperations(ILogger<MicrosoftOperations> logger, IOperationManager opsMan)
        {
            _logger = logger;
            _opsMan = opsMan;
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
    }
}
