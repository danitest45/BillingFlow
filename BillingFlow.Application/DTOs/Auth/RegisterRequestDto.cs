using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
