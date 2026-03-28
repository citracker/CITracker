using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class CancelSubscriptionRequest
    {
        public string StrId { get; set; }
        public string Provider { get; set; }
    }
}
