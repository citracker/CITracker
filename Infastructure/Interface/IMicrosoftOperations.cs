using Shared.DTO;
using Shared.ExternalModels;
using DriveInfo = Shared.ExternalModels.DriveInfo;

namespace Infastructure.Interface
{
    public interface IMicrosoftOperations
    {
        Task<List<DriveInfo>> DiscoverSharePointSites(string tenantId, string clientId, string clientSecret);
        Task<string> GetOrganizationDomain(string accesstoken);
        Task<MarketplaceSubscription> ResolveAsync(string token, string tenantId);
        Task ActivateAsync(string subsId, string tenantId);
        Task<CIMarketplaceSubscription> GetSubscription(string subsId, string tenantId);
    }
}
