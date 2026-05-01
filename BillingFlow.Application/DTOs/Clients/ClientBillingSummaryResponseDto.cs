using BillingFlow.Application.DTOs.Invoices;
using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Clients
{
    public class ClientBillingSummaryResponseDto
    {
        public ClientBillingInfoDto Client { get; set; } = new();
        public InvoiceResponseDto? CurrentInvoice { get; set; }
        public DateTime? NextDueDate { get; set; }
        public List<InvoiceResponseDto> History { get; set; } = new();
    }

    public class ClientBillingInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal MonthlyAmount { get; set; }
        public int DueDay { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public DateTime BillingStartDate { get; set; }
    }
}
