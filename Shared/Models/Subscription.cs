using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("Subscription")]
    public class Subscription
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public string PaymentCustomerId { get; set; }
        public string PaymentSubscriptionId { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public long LastUpdatedBy { get; set; }
    }
}
