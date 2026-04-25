using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Auth
{
    public class ForgotPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
