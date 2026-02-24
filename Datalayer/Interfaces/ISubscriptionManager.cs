using Shared.DTO;
using Shared.Models;

namespace Datalayer.Interfaces
{
    public interface ISubscriptionManager
    {
        Task<ResponseHandler<SubscriptionPlan>> GetAllSubscriptionPlans();
        Task<ResponseHandler<SubscriptionPlan>> GetSubscriptionPlanById(int id);
        Task<ResponseHandler<Organization>> GetOrganizationByTenantId(string tenantId);
        Task<ResponseHandler<OrganizationSubscription>> GetOrganizationSubscription(string tenantId);
        Task<ResponseHandler> RegisterOrganizationSubscription(Organization org, CIUser usr, Payment pay, Subscription sub);
    }
}
