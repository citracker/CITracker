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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
