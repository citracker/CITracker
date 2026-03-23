using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalModels
{
    public class Webhook
    {
        public string Id { get; set; }                 // Event ID
        public string ActivityId { get; set; }         // Correlation ID
        public string SubscriptionId { get; set; }     // Core identifier
        public string Action { get; set; }             // ChangePlan, Unsubscribe, etc.
        public DateTime TimeStamp { get; set; }        // Event time
    }
}
