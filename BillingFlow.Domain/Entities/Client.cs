using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Entities
{
    public class Client
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public decimal MonthlyAmount { get; set; }

        public int DueDay { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<InvoiceRecord> InvoiceRecords { get; set; } = new List<InvoiceRecord>();
    }
}
