using Shared.DTO;
using Shared.ExternalModels;
using Shared.Models;
using Stripe;
using Subscription = Shared.Models.Subscription;

namespace Datalayer.Interfaces
{
    public interface ISubscriptionManager
    {
        Task<ResponseHandler<SubscriptionPlan>> GetAllSubscriptionPlans();
        Task<ResponseHandler<SubscriptionPlan>> GetSubscriptionPlanById(int id);
        Task<ResponseHandler<SubscriptionPlan>> GetDefaultSubscriptionPlan();
        Task<ResponseHandler<SubscriptionPlan>> GetSubscriptionPlanByMarketPlaceId(string id);
        Task<ResponseHandler<Organization>> GetOrganizationByTenantId(string tenantId);
        Task<ResponseHandler<OrganizationSubscription>> GetOrganizationSubscription(string tenantId);
        Task<ResponseHandler<Organization>> RegisterOrganizationSubscription(Organization org, CIUser usr, Subscription sub);
        Task UpdateOrganizationSubscription(long orgId, string stripeCustomerId, string subStatus, long adminUser);
        Task UpdateOrganizationSubscriptionFromEvent(int clientReferenceId, string stripeCustomerId, string subscriptionId, string subscriptionStatus);
        Task UpdateOrganizationSubscriptionFromUpdatedEvent(string subscriptionId, string stripeCustomerId, DateTime? startDate, DateTime? endDate, string priceId, string subscriptionStatus, DateTime? trialStart, DateTime? trialEnd, long quantity, bool cancelAtPeriodEnd);
        Task UpdateOrganizationSubscriptionFromDeletedEvent(string subscriptionId, string subscriptionStatus);
        Task<ResponseHandler<Organization>> UpdateOrganizationSubscriptionFromPaymentSuceededEvent(string subscriptionId, string stripeCustomerId, DateTime? startDate, DateTime? endDate, DateTime? trialStartDate, DateTime? trialEndDate, string subscriptionStatus, decimal amount, string provider, string invoiceId, string paymentIntentId);
        Task UpdateOrganizationSubscriptionFromMPEvent(CIMarketplaceSubscription subscription);
        Task MPDeactivateOrganizationSubscription(CIMarketplaceSubscription subscription);

        // Pending subscription
        Task<ResponseHandler<PendingSubscription>> CreatePendingSubscription(PendingSubscription pending);
        Task<PendingSubscription> GetPendingSubscription(long pendingId);
        Task<PendingSubscription?> GetPendingSubscriptionByStripeCustomer(string stripeCustomerId);

        Task<PendingSubscription> GetPendingSubscriptionByStripeSession(string sessionId);
        Task UpdatePendingSubscription(PendingSubscription pending);
        Task MarkPendingSubscriptionLinked(long pendingId, int organizationId);

        // Webhook idempotency
        Task<bool> MarkWebhookEventProcessedAsync(string provider, string eventId);

        // Identity
        Task<ResponseHandler> UpsertUserIdentity(UserIdentity identity);
        Task<UserIdentity> GetUserIdentity(string provider, string externalId);
        Task UpdateOrganizationSubscriptionFromMPEventSeats(string subscriptionId, int newSeats);
    }
}
