using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class MonthlyProjectsByPhaseDTO
    {
        public List<string> labels { get; set; } = new();
        public List<PhaseDatasetDTO> datasets { get; set; } = new();
    }

    public class PhaseDatasetDTO
    {
        public string phase { get; set; } = default!;
        public List<int> data { get; set; } = new();
    }

    public class MonthlyPhaseRaw
    {
        public int YearNumber { get; set; }
        public int MonthNumber { get; set; }
        public string MonthLabel { get; set; } = default!;
        public string Phase { get; set; } = default!;
        public int TotalProjects { get; set; }
    }
}
