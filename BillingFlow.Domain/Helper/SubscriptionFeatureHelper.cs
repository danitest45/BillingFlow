using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Helper
{
    public static class SubscriptionFeatureHelper
    {
        public static bool CanUseWhatsApp(Subscription subscription)
        {
            return SubscriptionAccessHelper.IsSubscriptionActive(subscription)
                   && subscription.PlanType != PlanType.Trial;
        }

        public static void EnsureCanUseWhatsApp(Subscription? subscription)
        {
            if (subscription == null)
                throw new Exception("Assinatura não encontrada.");

            if (!SubscriptionAccessHelper.IsSubscriptionActive(subscription))
                throw new Exception("Sua assinatura está inativa. Faça upgrade para continuar usando o BillingFlow.");

            if (subscription.PlanType == PlanType.Trial)
                throw new Exception("Cobrança via WhatsApp está disponível a partir do plano Starter.");
        }
    }
}
