using Shared.DTO;
using Stripe;
using Stripe.Checkout;

namespace Infastructure.Interface
{
    public interface IStripePayment
    {
        Task<string> CreateCheckout(string uid, string stripeCustomerId, string stripePriceId, int qty, int trial);
        Task<ResponseHandler> CancelSubscription(string subscriptionId);
        Task<string> CreateCustomerPortal(string customerId);
        Task<Customer> CreateStripeCustomer(string email, string uid);
        string BuildPaymentLinkUrl(string paymentLinkUrl, long pendingId, string email, int seats);
        Task<Session> GetCheckoutSession(string sessionId);
        Task<string> CreateSeatUpgradeCheckout(string customerId, string priceId, int quantity, string successUrl);

    }
}
