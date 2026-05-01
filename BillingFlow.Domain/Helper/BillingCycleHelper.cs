using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Helper
{
    public static class BillingCycleHelper
    {
        public static bool ShouldGenerateForMonth(Client client, int year, int month)
        {
            var start = client.BillingStartDate;

            var monthsDiff = ((year - start.Year) * 12) + (month - start.Month);

            if (monthsDiff < 0)
                return false;

            return client.BillingCycle switch
            {
                BillingCycle.Monthly => true,
                BillingCycle.Quarterly => monthsDiff % 3 == 0,
                BillingCycle.SemiAnnual => monthsDiff % 6 == 0,
                BillingCycle.Annual => monthsDiff % 12 == 0,
                _ => true
            };
        }
    }
}
