using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("Payment")]
    public class Payment
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public string Provider { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
