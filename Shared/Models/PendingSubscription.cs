using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("PendingSubscription")]
    public class PendingSubscription
    {
        [ExplicitKey]  
        public long Id { get; set; }
        public int? OrganizationId { get; set; }
        public int PlanId { get; set; }
        public int SeatsRequested { get; set; }
        public string Provider { get; set; }
        public string ProviderCustomerId { get; set; }
        public string ProviderSubscriptionId { get; set; }
        public string StripeSessionId { get; set; }
        public string BillingEmail { get; set; }
        public int TrialDays { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}
