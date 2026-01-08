using Azure.Identity;
using Infastructure.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;
using Shared.DTO;
using Shared.ExternalModels;
using System.Net;
using System.Runtime.CompilerServices;

namespace Infastructure.Implementation
{
    public class MicrosoftOperations : IMicrosoftOperations
    {
        private readonly ILogger<MicrosoftOperations> _logger;
        public MicrosoftOperations(ILogger<MicrosoftOperations> logger)
        {
            _logger = logger;
        }

        public async Task<Shared.DTO.ResponseHandler<SiteInfo>> SearchSharePointSites(string tenantId, string clientId, string clientSecret, string searchTerm)
        {
            try
            {
                var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

                // Initialize the GraphServiceClient
                var graphClient = new GraphServiceClient(clientSecretCredential);

                // Start the search
                var sites = await graphClient.Sites.GetAsync(config =>
                {
                    config.QueryParameters.Search = searchTerm;
                });

                var allSites = new List<SiteInfo>();

                if (sites.Value != null)
                {
                    allSites.AddRange(sites.Value.Select(s => new SiteInfo
                    {
                        SiteId = s.Id,
                        Name = s.DisplayName,
                        WebUrl = s.WebUrl
                    }));
                }

                // Pagination
                var nextPage = sites.OdataNextLink;
                while (!string.IsNullOrEmpty(nextPage))
                {
                    var nextSites = await graphClient.Sites.GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Top = 50;
                    });

                    if (nextSites.Value != null)
                    {
                        allSites.AddRange(nextSites.Value.Select(s => new SiteInfo
                        {
                            SiteId = s.Id,
                            Name = s.DisplayName,
                            WebUrl = s.WebUrl
                        }));
                    }

                    nextPage = nextSites.OdataNextLink;
                }

                if (allSites.Any())
                {
                    return new Shared.DTO.ResponseHandler<SiteInfo>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Sites retrieved successfully",
                        Result = allSites
                    };
                }
                else
                {
                    return new Shared.DTO.ResponseHandler<SiteInfo>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "No sites found"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(SearchSharePointSites)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new Shared.DTO.ResponseHandler<SiteInfo>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }
    }
}
