namespace Shared.DTO
{
    public class OrganizationSubscription
    {
        public long OrganizationId { get; set; }
        public string Provider { get; set; }
        public int SubscriptionPlanId { get; set; }
        public string SubscriptionStatus { get; set; }
        public string SubscriptionName { get; set; }
        public string PaymentSubscriptionId { get; set; }
        public string PaymentCustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfLicences { get; set; }
        public int NumberOfUsedLicences { get; set; }
        public int SeatsPurchased { get; set; }
        public int SeatsAllocated { get; set; }
        public int MaxSeats { get; set; }
        public int MinSeats { get; set; }
        public DateTime? TrialEndUtc { get; set; }
    }
}
