using Shared.DTO;
using Stripe;

namespace Infastructure.Interface
{
    public interface IStripePayment
    {
        Task<string> CreateCheckout(string uid, string stripeCustomerId, string stripePriceId, int qty);
        Task<ResponseHandler> CancelSubscription(string subscriptionId);
        Task<string> CreateCustomerPortal(string customerId);
        Task<Customer> CreateStripeCustomer(string email, string uid);
    }
}
