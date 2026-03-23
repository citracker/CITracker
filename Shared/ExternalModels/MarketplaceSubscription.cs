namespace Shared.ExternalModels
{
    public class MarketplaceSubscription
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string OfferId { get; set; }
        public string PlanId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public Purchaser Purchaser { get; set; }
    }

    public class Purchaser
    {
        public string EmailId { get; set; }
        public string TenantId { get; set; }
    }

    public class Term
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }


    public class CIMarketplaceSubscription
    {
        public string Id { get; set; }
        public string PlanId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public Term Term { get; set; }
    }
}
