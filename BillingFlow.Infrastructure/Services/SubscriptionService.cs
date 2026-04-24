using BillingFlow.Application.DTOs.Subscription;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Enums;
using BillingFlow.Domain.Helper;
using BillingFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly BillingFlowDbContext _context;

        public SubscriptionService(BillingFlowDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionInfoDto> GetCurrentAsync(Guid userId)
        {
            var subscription = await _context.Subscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (subscription == null)
                throw new Exception("Assinatura não encontrada.");

            if (subscription.EndsAt.HasValue &&
                subscription.EndsAt.Value <= DateTime.UtcNow &&
                subscription.Status != SubscriptionStatus.Expired)
            {
                subscription.Status = SubscriptionStatus.Expired;
                await _context.SaveChangesAsync();
            }

            var totalClients = await _context.Clients
                .AsNoTracking()
                .CountAsync(c => c.UserId == userId);

            return new SubscriptionInfoDto
            {
                Plan = subscription.PlanType.ToString(),
                MaxClients = subscription.MaxClients,
                CurrentClients = totalClients,
                EndsAt = subscription.EndsAt,
                Status = subscription.Status.ToString()
            };
        }

        public async Task<SubscriptionInfoDto> UpgradeAsync(Guid userId, UpgradeSubscriptionRequestDto request)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (subscription == null)
                throw new Exception("Assinatura não encontrada.");

            if (request.PlanType == PlanType.Trial)
                throw new Exception("Não é permitido fazer upgrade para Trial.");

            var newMaxClients = PlanRulesHelper.GetMaxClients(request.PlanType);


            var totalClients = await _context.Clients
                .AsNoTracking()
                .CountAsync(c => c.UserId == userId);

            if (totalClients > newMaxClients)
            {
                throw new Exception(
                    $"Você possui {totalClients} clientes cadastrados e o plano selecionado permite apenas {newMaxClients}.");
            }

            subscription.PlanType = request.PlanType;
            subscription.Status = SubscriptionStatus.Active;
            subscription.MaxClients = PlanRulesHelper.GetMaxClients(request.PlanType);
            subscription.StartsAt = DateTime.UtcNow;
            subscription.EndsAt = DateTime.UtcNow.AddMonths(1);

            await _context.SaveChangesAsync();

            return new SubscriptionInfoDto
            {
                Plan = subscription.PlanType.ToString(),
                MaxClients = subscription.MaxClients,
                CurrentClients = totalClients,
                EndsAt = subscription.EndsAt,
                Status = subscription.Status.ToString()
            };
        }
    }
}
