using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Enums;
using BillingFlow.Domain.Helper;
using BillingFlow.Infrastructure.Persistence;
using BillingFlow.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;


namespace BillingFlow.Infrastructure.Services
{
    public class StripeWebhookService : IStripeWebhookService
    {
        private readonly BillingFlowDbContext _context;
        private readonly StripeSettings _stripeSettings;

        public StripeWebhookService(
            BillingFlowDbContext context,
            IOptions<StripeSettings> stripeOptions)
        {
            _context = context;
            _stripeSettings = stripeOptions.Value;
        }

        public async Task HandleEventAsync(string json, string stripeSignature)
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                _stripeSettings.WebhookSecret);

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompletedAsync(stripeEvent);
                    break;

                case "invoice.paid":
                    await HandleInvoicePaidAsync(stripeEvent);
                    break;

                case "customer.subscription.updated":
                    await HandleCustomerSubscriptionUpdatedAsync(stripeEvent);
                    break;

                case "customer.subscription.deleted":
                    await HandleCustomerSubscriptionDeletedAsync(stripeEvent);
                    break;
            }
        }

        private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;

            if (session == null)
                return;

            if (!session.Metadata.TryGetValue("userId", out var userIdValue))
                return;

            if (!Guid.TryParse(userIdValue, out var userId))
                return;

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (subscription == null)
                return;

            if (session.Metadata.TryGetValue("planType", out var planTypeValue) &&
                Enum.TryParse<PlanType>(planTypeValue, out var planType))
            {
                subscription.PlanType = planType;
                subscription.MaxClients = PlanRulesHelper.GetMaxClients(planType);
            }

            subscription.ProviderCustomerId = session.CustomerId;
            subscription.ProviderSubscriptionId = session.SubscriptionId;
            subscription.Status = SubscriptionStatus.Active;

            await _context.SaveChangesAsync();
        }

        private async Task HandleInvoicePaidAsync(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;

            if (invoice == null)
                return;

            var stripeSubscriptionId = invoice.Parent?.SubscriptionDetails?.Subscription?.Id;

            if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
                return;

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(x => x.ProviderSubscriptionId == stripeSubscriptionId);

            if (subscription == null)
                return;

            subscription.Status = SubscriptionStatus.Active;

            if (invoice.Lines?.Data?.Any() == true)
            {
                var period = invoice.Lines.Data[0].Period;

                if (period != null)
                {
                    subscription.StartsAt = DateTime.SpecifyKind(period.Start, DateTimeKind.Utc);
                    subscription.EndsAt = DateTime.SpecifyKind(period.End, DateTimeKind.Utc);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task HandleCustomerSubscriptionUpdatedAsync(Event stripeEvent)
        {
            var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;

            if (stripeSubscription == null)
                return;

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(x => x.ProviderSubscriptionId == stripeSubscription.Id);

            if (subscription == null)
                return;

            subscription.Status = MapStripeStatus(stripeSubscription.Status);

            var firstItem = stripeSubscription.Items?.Data?.FirstOrDefault();

            if (firstItem != null)
            {
                subscription.StartsAt = DateTime.SpecifyKind(firstItem.CurrentPeriodStart, DateTimeKind.Utc);
                subscription.EndsAt = DateTime.SpecifyKind(firstItem.CurrentPeriodEnd, DateTimeKind.Utc);
            }

            await _context.SaveChangesAsync();
        }

        private async Task HandleCustomerSubscriptionDeletedAsync(Event stripeEvent)
        {
            var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;

            if (stripeSubscription == null)
                return;

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(x => x.ProviderSubscriptionId == stripeSubscription.Id);

            if (subscription == null)
                return;

            subscription.Status = SubscriptionStatus.Canceled;

            await _context.SaveChangesAsync();
        }

        private static SubscriptionStatus MapStripeStatus(string stripeStatus)
        {
            return stripeStatus switch
            {
                "trialing" => SubscriptionStatus.Trialing,
                "active" => SubscriptionStatus.Active,
                "canceled" => SubscriptionStatus.Canceled,
                "past_due" => SubscriptionStatus.Expired,
                "unpaid" => SubscriptionStatus.Expired,
                "incomplete_expired" => SubscriptionStatus.Expired,
                _ => SubscriptionStatus.Expired
            };
        }
    }
}
