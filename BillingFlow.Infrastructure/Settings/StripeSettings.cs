using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Infrastructure.Settings
{
    public class StripeSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string StarterPriceId { get; set; } = string.Empty;
        public string ProPriceId { get; set; } = string.Empty;
        public string AgencyPriceId { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string PortalReturnUrl { get; set; } = string.Empty;
        public string PortalConfigurationId { get; set; } = string.Empty;
    }
}
