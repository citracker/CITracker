using Shared.DTO;
using Shared.Models;
using Stripe;
using Subscription = Shared.Models.Subscription;

namespace Datalayer.Interfaces
{
    public interface ISubscriptionManager
    {
        Task<ResponseHandler<SubscriptionPlan>> GetAllSubscriptionPlans();
        Task<ResponseHandler<SubscriptionPlan>> GetSubscriptionPlanById(int id);
        Task<ResponseHandler<Organization>> GetOrganizationByTenantId(string tenantId);
        Task<ResponseHandler<OrganizationSubscription>> GetOrganizationSubscription(string tenantId);
        Task<ResponseHandler> RegisterOrganizationSubscription(Organization org, CIUser usr, Subscription sub);
        Task UpdateOrganizationSubscription(long orgId, string stripeCustomerId, string subStatus, long adminUser);
        Task UpdateOrganizationSubscriptionFromEvent(int clientReferenceId, string stripeCustomerId, string subscriptionId, string subscriptionStatus);
        Task UpdateOrganizationSubscriptionFromUpdatedEvent(string subscriptionId, string stripeCustomerId, DateTime? startDate, DateTime? endDate, string priceId, string subscriptionStatus);
        Task UpdateOrganizationSubscriptionFromDeletedEvent(string subscriptionId, string subscriptionStatus);
        Task<ResponseHandler<Organization>> UpdateOrganizationSubscriptionFromPaymentSuceededEvent(string subscriptionId, string stripeCustomerId, DateTime? startDate, DateTime? endDate, string subscriptionStatus, decimal amount, string provider, string invoiceId, string paymentIntentId);
    }
}
