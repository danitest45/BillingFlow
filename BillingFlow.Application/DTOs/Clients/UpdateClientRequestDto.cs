using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Clients
{
    public class UpdateClientRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal MonthlyAmount { get; set; }
        public int DueDay { get; set; }
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
        public DateTime? BillingStartDate { get; set; }
    }
}
