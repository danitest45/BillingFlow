using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string to, string resetLink);
    }
}
