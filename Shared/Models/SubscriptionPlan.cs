using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("SubscriptionPlan")]
    public class SubscriptionPlan
    {
        [ExplicitKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Duration { get; set; }
        public decimal Amount { get; set; }
        public int FreeTrialDuration { get; set; }
        public int NumberOfLicences { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
