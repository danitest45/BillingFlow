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
            return subscription.Status == SubscriptionStatus.Active ||
                   subscription.Status == SubscriptionStatus.Trialing;
        }

        public static void EnsureSubscriptionIsActive(Subscription? subscription)
        {
            if (subscription == null)
                throw new Exception("Assinatura não encontrada.");

            if (!IsSubscriptionActive(subscription))
                throw new Exception("Sua assinatura está inativa. Faça upgrade para continuar usando o BillingFlow.");
        }
    }
}
