using BillingFlow.Application.DTOs.Billing;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Enums;
using BillingFlow.Infrastructure.Persistence;
using BillingFlow.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Text;
using Stripe.Checkout;

namespace BillingFlow.Infrastructure.Services
{
    public class BillingService : IBillingService
    {
        private readonly BillingFlowDbContext _context;
        private readonly StripeSettings _stripeSettings;

        public BillingService(
            BillingFlowDbContext context,
            IOptions<StripeSettings> stripeOptions)
        {
            _context = context;
            _stripeSettings = stripeOptions.Value;
        }

        public async Task<CreateCheckoutSessionResponseDto> CreateCheckoutSessionAsync(
            Guid userId,
            CreateCheckoutSessionRequestDto request)
        {
            if (request.PlanType == PlanType.Trial)
                throw new Exception("Não é permitido criar checkout para Trial.");

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (subscription == null)
                throw new Exception("Assinatura não encontrada.");

            var priceId = request.PlanType switch
            {
                PlanType.Starter => _stripeSettings.StarterPriceId,
                PlanType.Pro => _stripeSettings.ProPriceId,
                PlanType.Agency => _stripeSettings.AgencyPriceId,
                _ => throw new Exception("Plano inválido.")
            };

            var options = new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = _stripeSettings.SuccessUrl,
                CancelUrl = _stripeSettings.CancelUrl,
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    ["userId"] = userId.ToString(),
                    ["planType"] = request.PlanType.ToString()
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return new CreateCheckoutSessionResponseDto
            {
                Url = session.Url
            };
        }

        public async Task<CreateCustomerPortalSessionResponseDto> CreateCustomerPortalSessionAsync(Guid userId)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (subscription == null)
                throw new Exception("Assinatura não encontrada.");

            if (string.IsNullOrWhiteSpace(subscription.ProviderCustomerId))
                throw new Exception("Cliente Stripe não encontrado para esta conta.");

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = subscription.ProviderCustomerId,
                ReturnUrl = _stripeSettings.PortalReturnUrl,
                Configuration = _stripeSettings.PortalConfigurationId

            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options);

            return new CreateCustomerPortalSessionResponseDto
            {
                Url = session.Url
            };
        }

        public async Task<CreateCustomerPortalSessionResponseDto> CreateSubscriptionUpdatePortalSessionAsync(Guid userId)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (subscription == null)
                throw new Exception("Assinatura não encontrada.");

            if (string.IsNullOrWhiteSpace(subscription.ProviderCustomerId))
                throw new Exception("Cliente Stripe não encontrado para esta conta.");

            if (string.IsNullOrWhiteSpace(subscription.ProviderSubscriptionId))
                throw new Exception("Assinatura Stripe não encontrada para esta conta.");

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = subscription.ProviderCustomerId,
                ReturnUrl = _stripeSettings.PortalReturnUrl,
                Configuration = _stripeSettings.PortalConfigurationId,
                FlowData = new Stripe.BillingPortal.SessionFlowDataOptions
                {
                    Type = "subscription_update",
                    SubscriptionUpdate = new Stripe.BillingPortal.SessionFlowDataSubscriptionUpdateOptions
                    {
                        Subscription = subscription.ProviderSubscriptionId
                    }
                }
            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options);

            return new CreateCustomerPortalSessionResponseDto
            {
                Url = session.Url
            };
        }
    }
}
