using Shared.Models;

namespace Shared.ViewModels
{
    public class CheckoutVM
    { 
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<PaymentProvider> PaymentProvider { get; set; }
        public List<Country> Country { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public int? PendingSeats { get; set; }
        public long? PendingId { get; set; }
    }
}
