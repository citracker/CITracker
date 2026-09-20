using CITracker.Helpers;
using Datalayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.Enumerations;
using Shared.ExternalModels;
using Shared.Utilities;
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
                case "customer.subscription.trial_will_end": await HandleTrialWillEnd(stripeEvent); break;
            }
            return Ok();
        }

        private async Task HandleCheckoutCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;

            _logger.LogInformation($"HandleCheckoutCompleted hit. session = {JsonConvert.SerializeObject(session)}");

            _logger.LogInformation($"Checkout completed. ClientReferenceId={session.ClientReferenceId}, " +
                $"SubscriptionId={session.SubscriptionId}, AmountTotal={session.AmountTotal}");

            if (!long.TryParse(session.ClientReferenceId, out var pendingId))
            {
                _logger.LogInformation("No client_reference_id on Stripe session.");
                return;
            }

            var pending = await _subManager.GetPendingSubscription(pendingId);
            if (pending == null)
            {
                _logger.LogInformation($"PendingSubscription {pendingId} not found.");
                return;
            }

            // Idempotency: don't reprocess an already-paid pending
            if (pending.Status == "PaidAwaitingTenant" || pending.Status == "Linked")
            {
                _logger.LogInformation($"Pending {pendingId} already in status {pending.Status}; skipping.");
                return;
            }

            pending.ProviderCustomerId = session.CustomerId;
            pending.ProviderSubscriptionId = session.SubscriptionId;
            pending.StripeSessionId = session.Id;
            pending.BillingEmail = session.CustomerDetails?.Email  ?? session.CustomerEmail ?? pending.BillingEmail;
            pending.SeatsRequested = pending.SeatsRequested;   // ⬅ always positive
            pending.Status = "PaidAwaitingTenant";

            await _subManager.UpdatePendingSubscription(pending);

            _logger.LogInformation($"PendingSubscription {pendingId} → PaidAwaitingTenant ({pending.SeatsRequested} seats).");

            _logger.LogInformation($"About to Refresh the Subscription for {session.SubscriptionId}");

            var freshSub = await new SubscriptionService().GetAsync(session.SubscriptionId);

            _logger.LogInformation($"Refreshed Subscription for {session.SubscriptionId} ||| {JsonConvert.SerializeObject(freshSub)}");

            await _subManager.UpdateOrganizationSubscriptionFromUpdatedEvent(
                freshSub.Id,
                freshSub.CustomerId,
                freshSub.Items.Data[0].CurrentPeriodStart,
                freshSub.Items.Data[0].CurrentPeriodEnd,
                freshSub.Items.Data[0].Price.Id,
                freshSub.Status,
                freshSub.TrialStart,
                freshSub.TrialEnd,
                freshSub.Items.Data[0].Quantity,
                freshSub.CancelAtPeriodEnd);
        }

        private async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;

            _logger.LogInformation($"HandleSubscriptionUpdated Event hit ||| {JsonConvert.SerializeObject(subscription)}");

            if (subscription?.Items?.Data == null || subscription.Items.Data.Count == 0)
            {
                _logger.LogWarning("subscription.updated: no items on subscription; skipping.");
                return;
            }

            var item = subscription.Items.Data[0];

            await _subManager.UpdateOrganizationSubscriptionFromUpdatedEvent(subscription.Id, subscription.CustomerId, item.CurrentPeriodStart, item.CurrentPeriodEnd, item.Price.Id, Utils.MapStripeStatus(subscription.Status), subscription.TrialStart, subscription.TrialEnd, item.Quantity, subscription.CancelAtPeriodEnd);
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
                _logger.LogInformation("invoice.payment_succeeded: no invoice on event.");
                return;
            }

            _logger.LogInformation($"HandlePaymentSucceeded. Invoice Details {JsonConvert.SerializeObject(invoice)}");

            var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
            if (string.IsNullOrEmpty(subscriptionId))
            {
                _logger.LogWarning($"Invoice {invoice.Id} has no parent subscription; ignoring.");
                return;
            }

            // ── 1. Is this a trial / $0 invoice? Skip the "real payment" path.
            var isTrialInvoice = invoice.BillingReason == "subscription_create" && (invoice.AmountPaid == 0 || invoice.Status == "paid" && invoice.Total == 0);

            var isZeroAmount = invoice.AmountPaid == 0;

            _logger.LogInformation($"PaymentSucceeded: invoice={invoice.Id}, sub={subscriptionId}, reason={invoice.BillingReason}, amountPaid={invoice.AmountPaid}, isTrialInvoice={isTrialInvoice}");

            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.GetAsync(subscriptionId);
            if (subscription == null || subscription.Items?.Data?.Count == 0)
            {
                _logger.LogWarning($"Subscription {subscriptionId} not found or has no items.");
                return;
            }

            _logger.LogInformation($"HandlePaymentSucceeded. Subscription Details {JsonConvert.SerializeObject(subscription)}");

            // ── 2. Trial $0 invoice: do NOT record a payment, do NOT flip to ACTIVE.
            if (isTrialInvoice || isZeroAmount)
            {
                _logger.LogInformation($"Trial invoice for {subscriptionId} ($0) — leaving subscription in {subscription.Status}. No payment recorded.");

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

            var result = await _subManager.UpdateOrganizationSubscriptionFromPaymentSuceededEvent(subscription.Id, subscription.CustomerId, item.CurrentPeriodStart, item.CurrentPeriodEnd, subscription.TrialStart, subscription.TrialEnd, Utils.MapStripeStatus(subscription.Status), invoice.AmountPaid / 100m, "Stripe", invoice.Id, invoice.Payments?.Data?.FirstOrDefault()?.Payment?.PaymentIntent?.Id);

            if (result?.StatusCode == (int)HttpStatusCode.OK && result.SingleResult != null)
            {
                _mail.sendEmail(result.SingleResult.AdminEmailAddress, "Welcome to CITracker", "CITracker", _mail.PopulateRegistrationBody(result.SingleResult.Name));
            }
            else
            {
                _logger.LogWarning($"PaymentSucceeded post-processing returned {result?.StatusCode}: {result?.Message}");
            }
        }

        private async Task HandleTrialWillEnd(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;

            _logger.LogInformation($"HandleTrialWillEnd Event hit ||| {JsonConvert.SerializeObject(subscription)}");

            if (subscription?.Items?.Data == null || subscription.Items.Data.Count == 0)
            {
                _logger.LogWarning("trial_will_end: no items on subscription; skipping.");
                return;
            }

            var item = subscription.Items.Data[0];

            var res = await _subManager.MarkTrialEndingAsync(subscription.Id, subscription.CustomerId, subscription.TrialEnd, item.Price.Id);

            if (res.StatusCode == (int)HttpStatusCode.OK && res.SingleResult != null)
            {
                // Notify the tenant admin that billing starts in 3 days.
                _mail.sendEmail(res.SingleResult.AdminEmailAddress, "Your CITracker trial ends soon", "CITracker", _mail.PopulateTrialEndingBody(res.SingleResult.Name, subscription.TrialEnd ?? DateTime.UtcNow.AddDays(3)));
            }
            else
            {
                _logger.LogWarning($"TrialWillEnd post-processing returned {res.StatusCode}: {res.Message}");
            }
        }

        private async Task HandlePaymentFailed(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;

            _logger.LogInformation($"HandlePaymentFailed Event hit ||| {JsonConvert.SerializeObject(invoice)}");

            if (invoice == null)
            {
                _logger.LogWarning("invoice.payment_failed: no invoice on event.");
                return;
            }

            var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
            if (string.IsNullOrEmpty(subscriptionId))
            {
                _logger.LogWarning($"Invoice {invoice.Id} has no parent subscription; ignoring.");
                return;
            }

            var attemptCount = invoice.AttemptCount;
            var nextAttempt = invoice.NextPaymentAttempt;
            var hostedUrl = invoice.HostedInvoiceUrl;

            var res = await _subManager.MarkPaymentFailedAsync(subscriptionId, invoice.CustomerId, invoice.Id, invoice.AmountDue / 100m, attemptCount, nextAttempt, hostedUrl);

            if (res.StatusCode == (int)HttpStatusCode.OK && res.SingleResult != null)
            {
                _mail.sendEmail(res.SingleResult.AdminEmailAddress, "Action required: CITracker payment failed", "CITracker", _mail.PopulatePaymentFailedBody(res.SingleResult.Name, invoice.AmountDue / 100m, attemptCount, hostedUrl));
            }
            else
            {
                _logger.LogWarning($"PaymentFailed post-processing returned {res.StatusCode}: {res.Message}");
            }
        }
    }
}
