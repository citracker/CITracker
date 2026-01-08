using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class StrategicInitiativeDTO
    {
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
        public long OwnerId { get; set; }
        public string Owner { get; set; }
        public string Teams { get; set; }
        public long ExecutiveSponsorId { get; set; }
        public string ExecutiveSponsor { get; set; }
        public string Status { get; set; }
        public decimal PercentageProgress { get; set; }
        public decimal CummulativeROI { get; set; }
        public string Currency { get; set; }
        public long OrganizationCountryId { get; set; }
        public string OrganizationCountry { get; set; }
        public long OrganizationFacilityId { get; set; }
        public string OrganizationFacility { get; set; }
        public long OrganizationDepartmentId { get; set; }
        public string OrganizationDepartment { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByStaff { get; set; }
    }
}
