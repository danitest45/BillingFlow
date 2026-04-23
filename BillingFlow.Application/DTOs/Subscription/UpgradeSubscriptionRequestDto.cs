using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Subscription
{
    public class UpgradeSubscriptionRequestDto
    {
        public PlanType PlanType { get; set; }
    }
}
