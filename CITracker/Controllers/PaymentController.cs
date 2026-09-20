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

            _logger.LogInformation(
                $"Checkout completed. ClientReferenceId={session.ClientReferenceId}, " +
                $"SubscriptionId={session.SubscriptionId}, AmountTotal={session.AmountTotal}");

            if (!long.TryParse(session.ClientReferenceId, out var pendingId))
            {
                _logger.LogWarning("No client_reference_id on Stripe session.");
                return;
            }

            var pending = await _subManager.GetPendingSubscription(pendingId);
            if (pending == null)
            {
                _logger.LogWarning($"PendingSubscription {pendingId} not found.");
                return;
            }

            // Idempotency: don't reprocess an already-paid pending
            if (pending.Status == "PaidAwaitingTenant" || pending.Status == "Linked")
            {
                _logger.LogInformation($"Pending {pendingId} already in status {pending.Status}; skipping.");
                return;
            }

            // ── Fetch the subscription to get the REAL seat count ──
            int seats = pending.SeatsRequested;   // fallback to what the user asked for
            try
            {
                var subService = new SubscriptionService();
                var stripeSub = await subService.GetAsync(session.SubscriptionId);

                var item = stripeSub.Items.Data.FirstOrDefault();
                if (item != null && item.Quantity > 0)
                {
                    seats = (int)item.Quantity;
                    _logger.LogInformation($"Extracted {seats} seats from Stripe subscription line item.");
                }
                else
                {
                    _logger.LogWarning($"Subscription {session.SubscriptionId} has no line item quantity; " +
                                       $"falling back to requested seats ({pending.SeatsRequested}).");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to fetch Stripe subscription {session.SubscriptionId}: {ex.Message}");
                // Don't abort — we still want to record the customer/subscription IDs.
            }

            pending.ProviderCustomerId = session.CustomerId;
            pending.ProviderSubscriptionId = session.SubscriptionId;
            pending.StripeSessionId = session.Id;
            pending.BillingEmail = session.CustomerDetails?.Email
                                             ?? session.CustomerEmail
                                             ?? pending.BillingEmail;
            pending.SeatsRequested = seats;   // ⬅ always positive
            pending.Status = "PaidAwaitingTenant";

            await _subManager.UpdatePendingSubscription(pending);

            _logger.LogInformation($"PendingSubscription {pendingId} → PaidAwaitingTenant ({seats} seats).");
        }

        private async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;

            _logger.LogInformation($"HandleSubscriptionUpdated Event hit ||| {JsonConvert.SerializeObject(subscription)}");

            await _subManager.UpdateOrganizationSubscriptionFromUpdatedEvent(subscription.Id, subscription.CustomerId, subscription.Items.Data[0].CurrentPeriodStart, subscription.Items.Data[0].CurrentPeriodEnd, subscription.Items.Data[0].Price.Id, subscription.Status, subscription.TrialStart, subscription.TrialEnd, subscription.Items.Data[0].Quantity, subscription.CancelAtPeriodEnd);
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
            if (invoice == null)
            {
                _logger.LogWarning("invoice.payment_succeeded: no invoice on event.");
                return;
            }

            var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
            if (string.IsNullOrEmpty(subscriptionId))
            {
                _logger.LogWarning($"Invoice {invoice.Id} has no parent subscription; ignoring.");
                return;
            }

            // ── 1. Is this a trial / $0 invoice? Skip the "real payment" path.
            var isTrialInvoice =
                invoice.BillingReason == "subscription_create" &&
                (invoice.AmountPaid == 0 || invoice.Status == "paid" && invoice.Total == 0);

            var isZeroAmount = invoice.AmountPaid == 0;

            _logger.LogInformation(
                $"PaymentSucceeded: invoice={invoice.Id}, sub={subscriptionId}, " +
                $"reason={invoice.BillingReason}, amountPaid={invoice.AmountPaid}, " +
                $"isTrialInvoice={isTrialInvoice}");

            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.GetAsync(subscriptionId);
            if (subscription == null || subscription.Items?.Data?.Count == 0)
            {
                _logger.LogWarning($"Subscription {subscriptionId} not found or has no items.");
                return;
            }

            // ── 2. Trial $0 invoice: do NOT record a payment, do NOT flip to ACTIVE.
            if (isTrialInvoice || isZeroAmount)
            {
                _logger.LogInformation(
                    $"Trial invoice for {subscriptionId} ($0) — " +
                    $"leaving subscription in {subscription.Status}. No payment recorded.");

                // Find the pending row by stripe customer (already stored by checkout.session.completed)
                var pending = await _subManager.GetPendingSubscriptionByStripeCustomer(subscription.CustomerId);
                if (pending != null)
                {
                    pending.TrialDays = (int)((subscription.TrialEnd - subscription.TrialStart)?.TotalDays ?? 0);
                    await _subManager.UpdatePendingSubscription(pending);
                }
                return;
            }

            // ── 3. Real paid invoice: apply the update
            var item = subscription.Items.Data[0];

            var result = await _subManager.UpdateOrganizationSubscriptionFromPaymentSuceededEvent(
                subscription.Id,
                subscription.CustomerId,
                item.CurrentPeriodStart,
                item.CurrentPeriodEnd,
                subscription.TrialStart,
                subscription.TrialEnd,
                MapStripeStatus(subscription.Status),   // ⬅ use actual status, not forced ACTIVE
                invoice.AmountPaid / 100m,
                "Stripe",
                invoice.Id,
                invoice.Payments?.Data?.FirstOrDefault()?.Payment?.PaymentIntent?.Id);

            if (result?.StatusCode == (int)HttpStatusCode.OK && result.SingleResult != null)
            {
                _mail.sendEmail(
                    result.SingleResult.AdminEmailAddress,
                    "Welcome to CITracker",
                    "CITracker",
                    _mail.PopulateRegistrationBody(result.SingleResult.Name));
            }
            else
            {
                _logger.LogWarning(
                    $"PaymentSucceeded post-processing returned {result?.StatusCode}: {result?.Message}");
            }
        }

        private static string MapStripeStatus(string s) => s switch
        {
            "trialing" => "TRIALING",
            "active" => "ACTIVE",
            "past_due" => "PAST_DUE",
            "unpaid" => "PAST_DUE",
            "incomplete" => "PENDING_CONFIRMATION",
            "incomplete_expired" => "CANCELLED",
            "canceled" => "CANCELLED",
            "paused" => "SUSPENDED",
            _ => "ACTIVE"
        };

        private async Task HandlePaymentFailed(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            _logger.LogInformation($"HandlePaymentFailed Event hit ||| {JsonConvert.SerializeObject(invoice)}");
            ///TODO
            //await _repo.MarkPaymentFailed(invoice.SubscriptionId);
        }
    }
}
