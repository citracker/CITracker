using Datalayer.Interfaces;
using Infastructure.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using Stripe;
using Stripe.Checkout;
using System.Net;
using System.Text;

namespace Infastructure.Implementation
{
    public class StripePayment : IStripePayment
    {
        private readonly ILogger<StripePayment> _logger;
        private readonly IOptions<StripeKeyValues> _config;
        private readonly IOperationManager _opsMan;

        public StripePayment(ILogger<StripePayment> logger, IOperationManager opsMan, IOptions<StripeKeyValues> config)
        {
            _logger = logger;
            _opsMan = opsMan;
            _config = config;
        }


        public async Task<ResponseHandler> CancelSubscription(string subscriptionId)
        {
            try
            {
                var service = new SubscriptionService();
                var res = await service.CancelAsync(subscriptionId, null);

                _logger.LogInformation($"Subscription cancellation Response ||| SubscriptionId : {subscriptionId} ||| {JsonConvert.SerializeObject(res)}");

                return new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Subscription cancelled successfully."
                };
            }
            catch(Exception ex) 
            { 
                _logger.LogError($"Exception at {nameof(CancelSubscription)} ||| {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occurred while cancelling the subscription."
                };
            }
        }

        public async Task<string> CreateCheckout(string uid, string stripeCustomerId, string stripePriceId, int qty, int trial)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    Mode = "subscription",
                    Customer = stripeCustomerId,
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = stripePriceId,
                            Quantity = qty
                        }
                    },
                    ClientReferenceId = uid,
                    SubscriptionData = new SessionSubscriptionDataOptions
                    {
                        TrialPeriodDays = trial
                    },
                    PaymentMethodCollection = "always",
                    SuccessUrl = _config.Value.SuccessCallBack,
                    CancelUrl = _config.Value.FailedCallBack
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return session.Url;
            }
            catch (Exception ex) 
            { 
                _logger.LogError($"Exception at {nameof(CreateCheckout)} ||| {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }

        public async Task<string> CreateCustomerPortal(string customerId)
        {
            try
            {
                var options = new Stripe.BillingPortal.SessionCreateOptions
                {
                    Customer = customerId,
                    ReturnUrl = _config.Value.Dashboard
                };

                var service = new Stripe.BillingPortal.SessionService();
                var session = await service.CreateAsync(options);

                return session.Url;
            }
            catch(Exception ex) 
            { 
                _logger.LogError($"Exception at {nameof(CreateCustomerPortal)} ||| {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }

        public async Task<Customer> CreateStripeCustomer(string email, string uid)
        {
            try
            {
                var customerService = new CustomerService();

                return await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Email = email,
                    Metadata = new Dictionary<string, string>
                    {
                        { "AppUserId", uid }
                    }
                });
            }
            catch (Exception ex) 
            { 
                _logger.LogError($"Exception at {nameof(CreateStripeCustomer)} ||| {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }

        public string BuildPaymentLinkUrl(string paymentLinkUrl, long pendingId, string email, int seats)
        {
            // Stripe Payment Links accept client_reference_id + prefilled_email.
            // Seat count is user-selected on the Stripe-hosted page (Adjustable Quantity).
            var sb = new StringBuilder(paymentLinkUrl);
            sb.Append(paymentLinkUrl.Contains('?') ? '&' : '?');
            sb.Append($"client_reference_id={Uri.EscapeDataString(pendingId.ToString())}");
            if (!string.IsNullOrWhiteSpace(email))
                sb.Append($"&prefilled_email={Uri.EscapeDataString(email)}");
            return sb.ToString();
        }

        public async Task<Session> GetCheckoutSession(string sessionId)
        {
            var svc = new SessionService();
            return await svc.GetAsync(sessionId);
        }

        public async Task<ResponseHandler> UpgradeToNextPlanAsync(string stripeSubscriptionId, string newPriceId, int fullLicenseCount)
        {
            try
            {
                var subService = new SubscriptionService();
                var sub = await subService.GetAsync(stripeSubscriptionId);
                if (sub == null)
                    return new ResponseHandler { StatusCode = 404, Message = "Subscription not found." };

                var item = sub.Items.Data.FirstOrDefault();
                if (item == null)
                    return new ResponseHandler { StatusCode = 500, Message = "Subscription has no items." };

                await subService.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
                {
                    Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions
                {
                    Id       = item.Id,
                    Price    = newPriceId,
                    Quantity = fullLicenseCount        // ⬅ always the full plan size
                }
            },
                    ProrationBehavior = "create_prorations"
                });

                return new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Upgraded to the new plan with {fullLicenseCount} licenses. Your card will be charged the prorated difference."
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe plan upgrade failed: {ex.Message}");
                return new ResponseHandler { StatusCode = (int)HttpStatusCode.BadRequest, Message = ex.Message };
            }
        }

        public async Task<ResponseHandler> CancelAtPeriodEndAsync(string stripeSubscriptionId, bool cancelAtPeriodEnd)
        {
            try
            {
                await new SubscriptionService().UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = cancelAtPeriodEnd
                });

                return new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = cancelAtPeriodEnd
                        ? "Subscription will be cancelled at the end of the current period."
                        : "Cancellation reversed. Subscription will renew normally."
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe cancel-at-period-end failed: {ex.Message}");
                return new ResponseHandler { StatusCode = (int)HttpStatusCode.BadRequest, Message = ex.Message };
            }
        }
    }
}
