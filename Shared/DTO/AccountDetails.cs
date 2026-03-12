using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class AccountDetails
    {
        public int ActiveProjectCount { get; set; }
        public int ClosedProjectCount { get; set; }
        public string Name { get; set; }
        public string AdminName { get; set; }
        public string AdminEmail { get; set; }
        public string AdminPhoneNumber { get; set; }
        public string Address { get; set; }
    }
}
