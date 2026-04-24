using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Helper
{
    public static class SubscriptionAccessHelper
    {
        public static bool IsSubscriptionActive(Subscription subscription)
        {
            if (subscription.EndsAt.HasValue &&
                subscription.EndsAt.Value <= DateTime.UtcNow)
            {
                return false;
            }

            return subscription.Status == SubscriptionStatus.Active ||
                   subscription.Status == SubscriptionStatus.Trialing ||
                   subscription.Status == SubscriptionStatus.Canceled;
        }

        public static void EnsureSubscriptionIsActive(Subscription? subscription)
        {
            if (subscription == null)
                throw new Exception("Assinatura não encontrada.");

            if (!IsSubscriptionActive(subscription))
                throw new Exception("Sua assinatura expirou. Faça upgrade para continuar usando o sistema.");
        }
    }
}
