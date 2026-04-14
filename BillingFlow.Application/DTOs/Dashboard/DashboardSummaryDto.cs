using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public decimal TotalExpected { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
        public int OverdueCount { get; set; }
    }
}
