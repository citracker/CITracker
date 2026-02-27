using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class MonthlyProjectsByDepartmentDTO
    {
        public List<string> labels { get; set; } = new();
        public List<DepartmentDatasetDTO> datasets { get; set; } = new();
    }
    public class DepartmentDatasetDTO
    {
        public string department { get; set; } = default!;
        public List<int> data { get; set; } = new();
    }

    public class MonthlyDepartmentRaw
    {
        public string MonthLabel { get; set; } = default!;
        public int MonthNumber { get; set; }
        public string Department { get; set; } = default!;
        public int TotalProjects { get; set; }
    }
}
