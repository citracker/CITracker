using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class DashboardAnalytics
    {
        public string Currency { get; set; }
        public int ProjectCount { get; set; }
        public int Audited { get; set; }
        public decimal TotalExpectedRevenue { get; set; }
        public decimal TotalHardSavings { get; set; }
    }
}
