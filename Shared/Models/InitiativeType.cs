using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    [Table("InitiativeType")]
    public class InitiativeType
    {
        [ExplicitKey] 
        public string Name { get; set; }
        public string Short { get; set; }
    }
}
