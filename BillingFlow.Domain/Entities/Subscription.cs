using BillingFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public PlanType PlanType { get; set; }

        public SubscriptionStatus Status { get; set; }

        public int MaxClients { get; set; }

        public DateTime StartsAt { get; set; }

        public DateTime? EndsAt { get; set; }

        public string? ProviderCustomerId { get; set; }

        public string? ProviderSubscriptionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
