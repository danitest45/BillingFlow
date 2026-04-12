using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public Guid UserId { get; set; }
    }
}
