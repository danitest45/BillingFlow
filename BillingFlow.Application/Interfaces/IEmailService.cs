using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string to, string resetLink);
        Task SendSupportEmailAsync(string name, string email, string subject, string message);
    }
}
