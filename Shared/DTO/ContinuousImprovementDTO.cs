using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class ContinuousImprovementDTO
    {
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Priority { get; set; }
        public string BusinessObjectiveAlignment { get; set; }
        public string ProblemStatement { get; set; }
        public string Methodology { get; set; }
        public string Certification { get; set; }
        public string TotalExpectedRevenue { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string Phase { get; set; }
        public int CountryId { get; set; }
        public string Country { get; set; }
        public int FacilityId { get; set; }
        public string Facility { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        public bool IsOneTimeSavings { get; set; }
        public bool IsCarryOverSavings { get; set; }
        public DateTime FinancialVerificationDate { get; set; }
        public string SupportingValueStream { get; set; }
        public string FinancialReportUrl { get; set; }
        public string FinancialReportComment { get; set; }
        public bool IsAudited { get; set; }
        public long AuditedBy { get; set; }
        public DateTime? AuditedDate { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByUser { get; set; }
    }
}
