using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Subscription
{
    public class SubscriptionInfoDto
    {
        public string Plan { get; set; } = string.Empty;
        public int MaxClients { get; set; }
        public int CurrentClients { get; set; }
        public DateTime? EndsAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
