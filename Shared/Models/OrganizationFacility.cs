using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("OrganizationFacility")]
    public class OrganizationFacility
    {
        [ExplicitKey]
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public long OrganizationCountryId { get; set; }
        public string Facility { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
