using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class MonthlyProjectsByPhaseDTO
    {
        public List<string> Labels { get; set; } = new();
        public List<PhaseDatasetDTO> Datasets { get; set; } = new();
    }

    public class PhaseDatasetDTO
    {
        public string Phase { get; set; } = default!;
        public List<int> Data { get; set; } = new();
    }
}
