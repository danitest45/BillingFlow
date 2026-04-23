using BillingFlow.Application.DTOs.Billing;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface IBillingService
    {
        Task<CreateCheckoutSessionResponseDto> CreateCheckoutSessionAsync(
            Guid userId,
            CreateCheckoutSessionRequestDto request);
        Task<CreateCustomerPortalSessionResponseDto> CreateCustomerPortalSessionAsync(Guid userId);
    }
}
