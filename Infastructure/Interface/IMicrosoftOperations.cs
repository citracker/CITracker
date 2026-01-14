using Shared.DTO;
using Shared.ExternalModels;
using DriveInfo = Shared.ExternalModels.DriveInfo;

namespace Infastructure.Interface
{
    public interface IMicrosoftOperations
    {
        Task<List<DriveInfo>> DiscoverSharePointSites(string tenantId, string clientId, string clientSecret);
    }
}
