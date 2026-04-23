using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Enums
{
    public enum SubscriptionStatus
    {
        Trialing = 1,
        Active = 2,
        Expired = 3,
        Canceled = 4
    }
}
