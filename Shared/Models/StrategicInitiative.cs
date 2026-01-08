
using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("StrategicInitiative")]
    public class StrategicInitiative
    {
        [ExplicitKey]
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public long OwnerId { get; set; }
        public long ExecutiveSponsorId { get; set; }
        public long OrganizationCountryId { get; set; }
        public long OrganizationFacilityId { get; set; }
        public long OrganizationDepartmentId { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
