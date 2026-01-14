using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Request
{
    public class CIRequest
    {
        public long Id { get; set; }
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
        public int FacilityId { get; set; }
        public int DepartmentId { get; set; }
        public string SupportingValueStream { get; set; }
    }
}
