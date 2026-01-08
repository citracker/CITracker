using Shared.DTO;
using Shared.ExternalModels;

namespace Infastructure.Interface
{
    public interface IMicrosoftOperations
    {
        Task<ResponseHandler<SiteInfo>> SearchSharePointSites(string tenantId, string clientId, string clientSecret, string searchTerm);
    }
}
