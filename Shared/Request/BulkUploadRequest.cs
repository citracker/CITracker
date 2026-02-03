using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Request
{
    public class BulkUploadRequest
    {
        public int UploadType { get; set; }
        public List<Dictionary<string, string>> Rows { get; set; }
    }

    public class BulkUser
    {
        public long? Id { get; set; }
        public int? OrganizationId { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? DateCreated { get; set; }
        public long? CreatedBy { get; set; }
    }

    public class BulkLocation
    {
        public string Country { get; set; }
        public string Facility { get; set; }
        public string Department { get; set; }
    }

    public class BulkOE
    {
        public string Title { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
        public string FacilitatorEmailAddress { get; set; }
        public string SponsorEmailAddress { get; set; }
        public string ExecutiveSponsorEmailAddress { get; set; }
        public string IsCarryOverProject { get; set; }
        public string SavingsClassification { get; set; }
        public decimal TargetSavings { get; set; }
        public string Currency { get; set; }
        public string Country { get; set; }
        public string Facility { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
    }

    public class BulkSI
    {
        public string Title { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string OwnerEmailAddress { get; set; }
        public string ExecutiveSponsorEmailAddress { get; set; }
        public string Country { get; set; }
        public string Facility { get; set; }
        public string Department { get; set; }
    }

    public class BulkCI
    {
        public string Title { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Priority { get; set; }
        public string BusinessObjectiveAlignment { get; set; }
        public string ProblemStatement { get; set; }
        public string Methodology { get; set; }
        public string Certification { get; set; }
        public decimal? TotalExpectedRevenue { get; set; } = 0;
        public string Currency { get; set; }
        public string Status { get; set; }
        public int Phase { get; set; }
        public string Country { get; set; }
        public string Facility { get; set; }
        public string Department { get; set; }
        public bool IsOneTimeSavings { get; set; }
        public bool IsCarryOverSavings { get; set; }
        public string? SupportingValueStream { get; set; }
    }
}
