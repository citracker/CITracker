using CITracker.Helpers;
using Datalayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.Enumerations;
using Stripe;
using Stripe.Checkout;
using System.Net;
using Event = Stripe.Event;
using Subscription = Stripe.Subscription;

namespace CITracker.Controllers
{
    [Route("api/webhooks/stripe")]
    public class PaymentController : Controller
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IOptions<StripeKeyValues> _config;
        private readonly ISubscriptionManager _subManager;
        private readonly Mailer _mail;

        public PaymentController(IOptions<StripeKeyValues> config, ISubscriptionManager subManager, ILogger<PaymentController> logger, Mailer mail)
        {
            _config = config;
            _subManager = subManager;
            _logger = logger;
            _mail = mail;
        }

        //[HttpPost]
        //public async Task<IActionResult> Handle()
        //{
        //    var json = await new StreamReader(Request.Body).ReadToEndAsync();

        //    _logger.LogInformation($"Stripe Event just arrived -- Raw Json ||| {json}");

        //    var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _config.Value.WebhookSecret);

        //    _logger.LogInformation($"Stripe Event ||| {JsonConvert.SerializeObject(stripeEvent)}");

        //    switch (stripeEvent.Type)
        //    {
        //        case "checkout.session.completed":
        //            await HandleCheckoutCompleted(stripeEvent);
        //            break;

        //        case "customer.subscription.created":
        //        case "customer.subscription.updated":
        //            await HandleSubscriptionUpdated(stripeEvent);
        //            break;

        //        case "customer.subscription.deleted":
        //            await HandleSubscriptionDeleted(stripeEvent);
        //            break;

        //        case "invoice.payment_succeeded":
        //            await HandlePaymentSucceeded(stripeEvent);
        //            break;

        //        case "invoice.payment_failed":
        //            await HandlePaymentFailed(stripeEvent);
        //            break;
        //    }

        //    return Ok();
        //}

        //private async Task HandleCheckoutCompleted(Event stripeEvent)
        //{
        //    var session = stripeEvent.Data.Object as Session;

        //    _logger.LogInformation($"HandleCheckoutCompleted Event hit |||  {JsonConvert.SerializeObject(session)}");

        //    await _subManager.UpdateOrganizationSubscriptionFromEvent(Convert.ToInt32(session.ClientReferenceId), session.CustomerId, session.SubscriptionId, SubscriptionStatus.PENDING_CONFIRMATION.ToString());
        //}

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var sig = Request.Headers["Stripe-Signature"];
            Event stripeEvent;
            try { stripeEvent = EventUtility.ConstructEvent(json, sig, _config.Value.WebhookSecret); }
            catch (Exception ex) { _logger.LogWarning($"Stripe signature verification failed: {ex.Message}"); return BadRequest(); }

            // Idempotency
            if (!await _subManager.MarkWebhookEventProcessedAsync("Stripe", stripeEvent.Id))
            {
                _logger.LogInformation($"Duplicate Stripe event {stripeEvent.Id} ignored.");
                return Ok();
            }

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed": await HandleCheckoutCompleted(stripeEvent); break;
                case "customer.subscription.created":
                case "customer.subscription.updated": await HandleSubscriptionUpdated(stripeEvent); break;
                case "customer.subscription.deleted": await HandleSubscriptionDeleted(stripeEvent); break;
                case "invoice.payment_succeeded": await HandlePaymentSucceeded(stripeEvent); break;
                case "invoice.payment_failed": await HandlePaymentFailed(stripeEvent); break;
            }
            return Ok();
        }

        private async Task HandleCheckoutCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            _logger.LogInformation($"Checkout completed. ClientReferenceId={session.ClientReferenceId} ||| {JsonConvert.SerializeObject(session)}");

            if (!long.TryParse(session.ClientReferenceId, out var pendingId))
            {
                _logger.LogWarning("No client_reference_id on Stripe session.");
                return;
            }

            var pending = await _subManager.GetPendingSubscription(pendingId);
            if (pending == null) { _logger.LogWarning($"PendingSubscription {pendingId} not found."); return; }

            pending.ProviderCustomerId = session.CustomerId;
            pending.ProviderSubscriptionId = session.SubscriptionId;
            pending.StripeSessionId = session.Id;
            pending.BillingEmail = session.CustomerDetails?.Email ?? session.CustomerEmail ?? pending.BillingEmail;
            pending.SeatsRequested = (int)((decimal)(session.AmountTotal / (decimal)(await _subManager.GetSubscriptionPlanById(pending.PlanId)).SingleResult.PricePerSeatPerYear));
            pending.Status = "PaidAwaitingTenant";
            await _subManager.UpdatePendingSubscription(pending);
        }

        private async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;

            _logger.LogInformation($"HandleSubscriptionUpdated Event hit ||| {JsonConvert.SerializeObject(subscription)}");

            await _subManager.UpdateOrganizationSubscriptionFromUpdatedEvent(subscription.Id, subscription.CustomerId, subscription.Items.Data[0].CurrentPeriodStart, subscription.Items.Data[0].CurrentPeriodEnd, subscription.Items.Data[0].Price.Id, subscription.Status);
        }

        private async Task HandleSubscriptionDeleted(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;

            _logger.LogInformation($"HandleSubscriptionDeleted Event hit ||| {JsonConvert.SerializeObject(subscription)}");

            await _subManager.UpdateOrganizationSubscriptionFromDeletedEvent(subscription.Id, SubscriptionStatus.CANCELLED.ToString());
        }

        private async Task HandlePaymentSucceeded(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            var subscriptionService = new SubscriptionService();

            var subscription = await subscriptionService.GetAsync(invoice?.Parent?.SubscriptionDetails?.SubscriptionId);
            _logger.LogInformation($"HandlePaymentSucceeded Event hit ||| {JsonConvert.SerializeObject(invoice)} ||| {JsonConvert.SerializeObject(subscription)}");

            var res = await _subManager.UpdateOrganizationSubscriptionFromPaymentSuceededEvent(subscription.Id, subscription.CustomerId, subscription.Items.Data[0].CurrentPeriodStart, subscription.Items.Data[0].CurrentPeriodEnd, SubscriptionStatus.ACTIVE.ToString(), Convert.ToDecimal((decimal)invoice.AmountPaid/(decimal)100), "Stripe", invoice.Id, invoice?.Payments?.Data?.FirstOrDefault()?.Payment?.PaymentIntent?.Id);

            if (res.StatusCode == (int)HttpStatusCode.OK)
            {
                //send Registration email
                _mail.sendEmail(res.SingleResult.AdminEmailAddress, "Welcome to CITracker", "CITracker", _mail.PopulateRegistrationBody(res.SingleResult.Name));
            }
        }

        private async Task HandlePaymentFailed(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            _logger.LogInformation($"HandlePaymentFailed Event hit ||| {JsonConvert.SerializeObject(invoice)}");
            ///TODO
            //await _repo.MarkPaymentFailed(invoice.SubscriptionId);
        }
    }
}
