using BillingFlow.Application.DTOs.Billing;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BillingFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession(
            [FromBody] CreateCheckoutSessionRequestDto request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _billingService.CreateCheckoutSessionAsync(userId, request);

            return Ok(result);
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook(
            [FromServices] IStripeWebhookService stripeWebhookService)
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            var json = await reader.ReadToEndAsync();

            var stripeSignature = Request.Headers["Stripe-Signature"];

            await stripeWebhookService.HandleEventAsync(json, stripeSignature!);

            return Ok();
        }

        [HttpPost("create-customer-portal-session")]
        public async Task<IActionResult> CreateCustomerPortalSession()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _billingService.CreateCustomerPortalSessionAsync(userId);

            return Ok(result);
        }

        [HttpPost("create-subscription-update-portal-session")]
        public async Task<IActionResult> CreateSubscriptionUpdatePortalSession()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _billingService.CreateSubscriptionUpdatePortalSessionAsync(userId);

            return Ok(result);
        }
    }
}
