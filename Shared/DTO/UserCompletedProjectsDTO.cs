using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class UserCompletedProjectsDTO
    {
        public string name { get; set; } = default!;
        public int completed { get; set; }
    }
}
