using BillingFlow.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    }
}
