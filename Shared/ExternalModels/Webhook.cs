using System.Text.Json.Serialization;

namespace Shared.ExternalModels
{
    public class Webhook
    {
        [JsonPropertyName("id")] 
        public string Id { get; set; }


        [JsonPropertyName("activityId")] 
        public string ActivityId { get; set; }


        [JsonPropertyName("subscriptionId")] 
        public string SubscriptionId { get; set; }


        [JsonPropertyName("publisherId")] 
        public string PublisherId { get; set; }


        [JsonPropertyName("offerId")] 
        public string OfferId { get; set; }


        [JsonPropertyName("planId")] 
        public string PlanId { get; set; }


        [JsonPropertyName("quantity")] 
        public int? Quantity { get; set; }


        [JsonPropertyName("action")]
        public string MarketplaceAction { get; set; }


        [JsonPropertyName("timeStamp")] 
        public DateTime TimeStamp { get; set; }


        [JsonPropertyName("operationId")] 
        public string OperationId { get; set; }
    }
}
