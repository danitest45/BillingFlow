using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Helper
{
    public static class PlanRulesHelper
    {
        public static int GetMaxClients(PlanType planType)
        {
            return planType switch
            {
                PlanType.Trial => 10,
                PlanType.Starter => 10,
                PlanType.Pro => 30,
                PlanType.Agency => 100,
                _ => 10
            };
        }
    }
}
