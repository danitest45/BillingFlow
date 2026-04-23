using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Entities
{
    public class MessageTemplate
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string ChargeTemplate { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
