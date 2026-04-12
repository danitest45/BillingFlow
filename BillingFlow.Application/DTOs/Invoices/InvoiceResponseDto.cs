using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Invoices
{
    public class InvoiceResponseDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
