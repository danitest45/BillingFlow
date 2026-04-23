using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface IStripeWebhookService
    {
        Task HandleEventAsync(string json, string stripeSignature);
    }
}
