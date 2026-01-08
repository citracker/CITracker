using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class InitiativeFilter
    {
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public long UserId { get; set; }
        public int CountryId { get; set; }
        public long DepartmentId { get; set; }
    }
}
