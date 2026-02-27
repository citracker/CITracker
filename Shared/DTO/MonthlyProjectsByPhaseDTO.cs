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
        public string Month { get; set; } = default!;
        public int MonthNumber { get; set; }
        public int Define { get; set; }
        public int Measure { get; set; }
        public int Analyze { get; set; }
        public int Improve { get; set; }
        public int Control { get; set; }
        public int ConceptInitiation { get; set; }
        public int DefinitionPlanning { get; set; }
        public int Execution { get; set; }
        public int PerformanceControl { get; set; }
        public int ProjectClosure { get; set; }
    }
}
