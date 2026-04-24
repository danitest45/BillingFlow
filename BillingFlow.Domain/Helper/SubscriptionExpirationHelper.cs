using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Helper
{
    public static class SubscriptionExpirationHelper
    {
        public static bool IsExpired(Subscription subscription, DateTime utcNow)
        {
            return subscription.EndsAt.HasValue &&
                   subscription.EndsAt.Value <= utcNow;
        }

        public static void ApplyExpirationIfNeeded(Subscription subscription, DateTime utcNow)
        {
            if (IsExpired(subscription, utcNow))
            {
                subscription.Status = SubscriptionStatus.Expired;
            }
        }

        public static bool CanUseSystem(Subscription subscription, DateTime utcNow)
        {
            if (IsExpired(subscription, utcNow))
                return false;

            return subscription.Status == SubscriptionStatus.Active ||
                   subscription.Status == SubscriptionStatus.Trialing ||
                   subscription.Status == SubscriptionStatus.Canceled;
        }
    }
}
