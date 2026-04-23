using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.MessageTemplate
{
    public class WhatsAppPreviewResponseDto
    {
        public string Phone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string WhatsAppUrl { get; set; } = string.Empty;
    }
}
