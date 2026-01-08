using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    [Table("AuditLog")]
    public class AuditLog
    {
        [ExplicitKey] 
        public long Id { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
    }
}
