using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class StripeKeyValues
    {
        public string SecretKey { get; set; }
        public string PublishableKey { get; set; }
        public string WebhookSecret { get; set; }
        public string BasicPriceId { get; set; }
        public string ProPriceId { get; set; }
        public string EnterprisePriceId { get; set; }
    }
}
