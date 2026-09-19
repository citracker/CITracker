
using Shared.Enumerations;

namespace Shared.Interfaces
{
    public interface IPaymentProvider
    {
        PaymentProviderKind Kind { get; }

        // Called when user initiates checkout (from the pricing page)
        Task<string> CreateCheckoutLinkAsync(CheckoutRequest request);

        //// Called by the webhook when payment succeeds
        //Task<ProviderSubscription> GetSubscriptionAsync(string providerSubscriptionId);

        // Mid-cycle seat/plan changes
        Task ChangeSeatsAsync(string providerSubscriptionId, int newSeats);
        Task ChangePlanAsync(string providerSubscriptionId, int newPlanId, int newSeats);

        // Cancellation
        Task CancelAsync(string providerSubscriptionId, bool atPeriodEnd);
    }

    public record CheckoutRequest(
        Guid PendingId,
        int PlanId,
        int Seats,
        int TrialDays,
        string BillingEmail,
        string SuccessUrl,
        string CancelUrl);
}
