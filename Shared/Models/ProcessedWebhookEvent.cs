using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("ProcessedWebhookEvent")]
    public class ProcessedWebhookEvent
    {
        [ExplicitKey]
        public long Id { get; set; }
        public string Provider { get; set; }
        public string EventId { get; set; }
        public DateTime ProcessedAtUtc { get; set; }
    }
}
