using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("OrganizationDepartment")]
    public class OrganizationDepartment
    {
        [ExplicitKey]
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public long OrganizationCountryId { get; set; }
        public long OrganizationFacilityId { get; set; }
        public string Department { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
