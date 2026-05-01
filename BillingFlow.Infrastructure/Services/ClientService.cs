using BillingFlow.Application.DTOs.Clients;
using BillingFlow.Application.DTOs.Common;
using BillingFlow.Application.DTOs.Invoices;
using BillingFlow.Application.Helper;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using BillingFlow.Domain.Helper;
using BillingFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Infrastructure.Services
{
    public class ClientService : IClientService
    {
        private readonly BillingFlowDbContext _context;

        public ClientService(BillingFlowDbContext context)
        {
            _context = context;
        }

        public async Task<ClientResponseDto> CreateAsync(
            Guid userId,
            CreateClientRequestDto request)
        {

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            SubscriptionAccessHelper.EnsureSubscriptionIsActive(subscription);

            if (subscription == null)
                throw new Exception("Assinatura não encontrada.");

            if (subscription.Status != SubscriptionStatus.Active &&
                subscription.Status != SubscriptionStatus.Trialing)
            {
                throw new Exception("Assinatura inativa.");
            }

            var totalClients = await _context.Clients
                .CountAsync(c => c.UserId == userId);

            if (SubscriptionHelper.HasReachedClientLimit(totalClients, subscription))
            {
                throw new Exception("Você atingiu o limite de clientes do seu plano.");
            }

            var client = new Client
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                MonthlyAmount = request.MonthlyAmount,
                DueDay = request.DueDay,
                IsActive = true,
                BillingStartDate = DateTime.SpecifyKind((request.BillingStartDate ?? DateTime.UtcNow).Date,DateTimeKind.Utc)
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return new ClientResponseDto
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email,
                Phone = client.Phone,
                MonthlyAmount = client.MonthlyAmount,
                DueDay = client.DueDay
            };
        }

        public async Task<PagedResultDto<ClientResponseDto>> GetAllAsync(
            Guid userId,
            ClientFilterRequestDto filter)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize > 100 ? 100 : filter.PageSize;

            var query = _context.Clients
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();

                query = query.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    x.Email.ToLower().Contains(search));
            }

            if (filter.DueDay.HasValue)
            {
                query = query.Where(x => x.DueDay == filter.DueDay.Value);
            }

            if (filter.MinMonthlyAmount.HasValue)
            {
                query = query.Where(x => x.MonthlyAmount >= filter.MinMonthlyAmount.Value);
            }

            if (filter.MaxMonthlyAmount.HasValue)
            {
                query = query.Where(x => x.MonthlyAmount <= filter.MaxMonthlyAmount.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ClientResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Phone = x.Phone,
                    MonthlyAmount = x.MonthlyAmount,
                    DueDay = x.DueDay
                })
                .ToListAsync();

            return new PagedResultDto<ClientResponseDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<ClientResponseDto> UpdateAsync(
            Guid userId,
            Guid clientId,
            UpdateClientRequestDto request)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.Id == clientId &&
                    x.UserId == userId);

            if (client is null)
                throw new Exception("Cliente não encontrado.");

            client.Name = request.Name;
            client.Email = request.Email;
            client.Phone = request.Phone;
            client.MonthlyAmount = request.MonthlyAmount;
            client.DueDay = request.DueDay;

            await _context.SaveChangesAsync();

            return new ClientResponseDto
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email,
                Phone = client.Phone,
                MonthlyAmount = client.MonthlyAmount,
                DueDay = client.DueDay
            };
        }

        public async Task DeleteAsync(Guid userId, Guid clientId)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.Id == clientId &&
                    x.UserId == userId);

            if (client is null)
                throw new Exception("Cliente não encontrado.");

            _context.Clients.Remove(client);

            await _context.SaveChangesAsync();
        }

        public async Task<ClientBillingSummaryResponseDto> GetBillingSummaryAsync(Guid userId, Guid clientId)
        {
            var client = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == clientId &&
                    x.UserId == userId);

            if (client is null)
                throw new Exception("Cliente não encontrado.");

            var now = DateTime.UtcNow;

            var currentInvoice = await _context.InvoiceRecords
                .AsNoTracking()
                .Where(i =>
                    i.ClientId == client.Id &&
                    i.ReferenceYear == now.Year &&
                    i.ReferenceMonth == now.Month)
                .Select(i => new InvoiceResponseDto
                {
                    Id = i.Id,
                    ClientId = i.ClientId,
                    ClientName = client.Name,
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    Status = i.Status,
                    PaidAt = i.PaidAt
                })
                .FirstOrDefaultAsync();

            var history = await _context.InvoiceRecords
                .AsNoTracking()
                .Where(i => i.ClientId == client.Id)
                .OrderByDescending(i => i.DueDate)
                .Take(12)
                .Select(i => new InvoiceResponseDto
                {
                    Id = i.Id,
                    ClientId = i.ClientId,
                    ClientName = client.Name,
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    Status = i.Status,
                    PaidAt = i.PaidAt
                })
                .ToListAsync();

            var nextDueDate = GetNextDueDate(client, now);

            return new ClientBillingSummaryResponseDto
            {
                Client = new ClientBillingInfoDto
                {
                    Id = client.Id,
                    Name = client.Name,
                    Email = client.Email,
                    Phone = client.Phone,
                    MonthlyAmount = client.MonthlyAmount,
                    DueDay = client.DueDay,
                    BillingCycle = client.BillingCycle,
                    BillingStartDate = client.BillingStartDate
                },
                CurrentInvoice = currentInvoice,
                NextDueDate = nextDueDate,
                History = history
            };
        }

        private static DateTime BuildDueDate(int year, int month, int dueDay)
        {
            var lastDayOfMonth = DateTime.DaysInMonth(year, month);
            var safeDueDay = dueDay > lastDayOfMonth ? lastDayOfMonth : dueDay;

            return new DateTime(year, month, safeDueDay, 12, 0, 0, DateTimeKind.Utc);
        }

        private static int GetCycleIntervalInMonths(BillingCycle billingCycle)
        {
            return billingCycle switch
            {
                BillingCycle.Monthly => 1,
                BillingCycle.Quarterly => 3,
                BillingCycle.SemiAnnual => 6,
                BillingCycle.Annual => 12,
                _ => 1
            };
        }

        private static DateTime GetNextDueDate(Client client, DateTime now)
        {
            var interval = GetCycleIntervalInMonths(client.BillingCycle);

            var start = DateTime.SpecifyKind(client.BillingStartDate.Date, DateTimeKind.Utc);

            var candidate = BuildDueDate(start.Year, start.Month, client.DueDay);

            while (candidate.Date < now.Date)
            {
                candidate = candidate.AddMonths(interval);
                candidate = BuildDueDate(candidate.Year, candidate.Month, client.DueDay);
            }

            return candidate;
        }
    }
}
