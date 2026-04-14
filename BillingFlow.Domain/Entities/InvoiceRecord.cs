using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Entities
{
    public class InvoiceRecord
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? PaidAt { get; set; }
        public int ReferenceYear { get; set; }
        public int ReferenceMonth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
