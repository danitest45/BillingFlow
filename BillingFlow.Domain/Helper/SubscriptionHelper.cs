using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Helper
{
    public static class SubscriptionHelper
    {
        public static bool CanCreateClient(Subscription sub)
            => sub.MaxClients > 0;

        public static bool CanGenerateInvoice(Subscription sub)
            => sub.Status == SubscriptionStatus.Active || sub.Status == SubscriptionStatus.Trialing;

        public static bool HasReachedClientLimit(int totalClients, Subscription sub)
            => totalClients >= sub.MaxClients;

        public static bool CanUseAdvancedFeatures(Subscription sub)
            => sub.PlanType != PlanType.Trial;
    }
}
