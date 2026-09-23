using Newtonsoft.Json;

namespace Shared.ExternalModels
{
    public class CIMarketplaceSubscription
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("subscriptionName")]
        public string SubscriptionName { get; set; }

        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("subscription")]
        public MPSubscription Subscription { get; set; }
    }

    public class MPSubscription
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("publisherId")]
        public string PublisherId { get; set; }

        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("saasSubscriptionStatus")]
        public string SaasSubscriptionStatus { get; set; }   // "Subscribed", "Unsubscribed", "Suspended", "PendingFulfillmentStart"

        [JsonProperty("beneficiary")]
        public Beneficiary Beneficiary { get; set; }

        [JsonProperty("purchaser")]
        public Purchaser Purchaser { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("term")]
        public Term Term { get; set; }

        [JsonProperty("autoRenew")]
        public bool AutoRenew { get; set; }

        [JsonProperty("isTest")]
        public bool IsTest { get; set; }

        [JsonProperty("isFreeTrial")]
        public bool IsFreeTrial { get; set; }

        [JsonProperty("allowedCustomerOperations")]
        public List<string> AllowedCustomerOperations { get; set; }

        [JsonProperty("sandboxType")]
        public string SandboxType { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("lastModified")]
        public DateTime? LastModified { get; set; }   // nullable, often 0001-01-01

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("sessionMode")]
        public string SessionMode { get; set; }   // "None" or "DryRun"
    }

    public class Beneficiary
    {
        [JsonProperty("emailId")]
        public string EmailId { get; set; }

        [JsonProperty("objectId")]
        public string ObjectId { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("puid")]
        public string Puid { get; set; }
    }

    public class Purchaser
    {
        [JsonProperty("emailId")]
        public string EmailId { get; set; }

        [JsonProperty("objectId")]
        public string ObjectId { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("puid")]
        public string Puid { get; set; }
    }

    public class Term
    {
        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("termUnit")]
        public string TermUnit { get; set; }   // "P1M", "P1Y", etc.
    }
}
