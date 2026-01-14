using Dapper.Contrib.Extensions;
namespace Shared.Models
{
    [Table("Organization")]
    public class Organization
    {
        [ExplicitKey] 
        public int Id { get; set; }
        public string Name { get; set; }
        public string TenantId { get; set; }
        public string AdminName { get; set; }
        public string AdminEmailAddress { get; set; }
        public string AdminPhoneNumber { get; set; }
        public string Provider { get; set; }
        public int CountryId { get; set; }
        public string Address { get; set; }
        public bool IsSubscribed { get; set; }
        public long? SubscriptionId { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }


        public string? SiteId { get; set; }
    }
}
