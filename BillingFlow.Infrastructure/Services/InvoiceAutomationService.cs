using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using BillingFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Infrastructure.Services
{
    public class InvoiceAutomationService : IInvoiceAutomationService
    {
        private readonly BillingFlowDbContext _context;

        public InvoiceAutomationService(BillingFlowDbContext context)
        {
            _context = context;
        }

        public async Task<int> GenerateMissingInvoicesForCurrentMonthAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var year = now.Year;
            var month = now.Month;

            var activeUserIds = await _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing)
                .Select(s => s.UserId)
                .ToListAsync(cancellationToken);

            var clients = await _context.Clients
                .AsNoTracking()
                .Where(c => c.IsActive && activeUserIds.Contains(c.UserId))
                .ToListAsync(cancellationToken);

            var existingInvoiceClientIds = await _context.InvoiceRecords
                .AsNoTracking()
                .Where(i => i.ReferenceYear == year && i.ReferenceMonth == month)
                .Select(i => i.ClientId)
                .ToListAsync(cancellationToken);

            var existingClientIdsSet = existingInvoiceClientIds.ToHashSet();

            var invoicesToCreate = new List<InvoiceRecord>();

            foreach (var client in clients)
            {
                if (existingClientIdsSet.Contains(client.Id))
                    continue;

                var lastDayOfMonth = DateTime.DaysInMonth(year, month);
                var dueDay = client.DueDay > lastDayOfMonth ? lastDayOfMonth : client.DueDay;

                var dueDate = new DateTime(year, month, dueDay, 12, 0, 0, DateTimeKind.Utc);

                var status = dueDate < now
                    ? PaymentStatus.Overdue
                    : PaymentStatus.Pending;

                invoicesToCreate.Add(new InvoiceRecord
                {
                    Id = Guid.NewGuid(),
                    ClientId = client.Id,
                    Amount = client.MonthlyAmount,
                    DueDate = dueDate,
                    Status = status,
                    ReferenceYear = year,
                    ReferenceMonth = month,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!invoicesToCreate.Any())
                return 0;

            await _context.InvoiceRecords.AddRangeAsync(invoicesToCreate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return invoicesToCreate.Count;
        }
    }
}
