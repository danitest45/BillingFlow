using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Infrastructure.Settings
{
    public class EmailSettings
    {
        public string From { get; set; } = string.Empty;
        public string FrontendUrl { get; set; } = string.Empty;
    }
}
