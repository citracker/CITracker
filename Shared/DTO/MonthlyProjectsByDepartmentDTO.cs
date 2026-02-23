using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class MonthlyProjectsByDepartmentDTO
    {
        public List<string> Labels { get; set; } = new();
        public List<DepartmentDatasetDTO> Datasets { get; set; } = new();
    }
    public class DepartmentDatasetDTO
    {
        public string Department { get; set; } = default!;
        public List<int> Data { get; set; } = new();
    }
}
