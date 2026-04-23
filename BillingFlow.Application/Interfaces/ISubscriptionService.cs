using BillingFlow.Application.DTOs.Subscription;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<SubscriptionInfoDto> GetCurrentAsync(Guid userId);
        Task<SubscriptionInfoDto> UpgradeAsync(Guid userId, UpgradeSubscriptionRequestDto request);
    }
}
