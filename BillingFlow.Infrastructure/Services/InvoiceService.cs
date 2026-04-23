using BillingFlow.Application.DTOs.Common;
using BillingFlow.Application.DTOs.Invoices;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using BillingFlow.Domain.Helper;
using BillingFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace BillingFlow.Infrastructure.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly BillingFlowDbContext _context;

        public InvoiceService(BillingFlowDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceResponseDto> GenerateAsync(Guid userId, Guid clientId)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            SubscriptionAccessHelper.EnsureSubscriptionIsActive(subscription);

            var client = await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.Id == clientId &&
                    x.UserId == userId);

            if (client is null)
                throw new Exception("Cliente não encontrado.");

            var now = DateTime.UtcNow;

            var existingInvoice = await _context.InvoiceRecords
                .FirstOrDefaultAsync(i =>
                    i.ClientId == client.Id &&
                    i.ReferenceYear == now.Year &&
                    i.ReferenceMonth == now.Month);

            if (existingInvoice is not null)
                throw new Exception("Já existe cobrança para este cliente no mês atual.");

            var lastDayOfMonth = DateTime.DaysInMonth(now.Year, now.Month);
            var dueDay = client.DueDay > lastDayOfMonth ? lastDayOfMonth : client.DueDay;

            var dueDate = new DateTime(now.Year, now.Month, dueDay, 12, 0, 0, DateTimeKind.Utc);

            var status = dueDate < now
                ? PaymentStatus.Overdue
                : PaymentStatus.Pending;

            var invoice = new InvoiceRecord
            {
                Id = Guid.NewGuid(),
                ClientId = client.Id,
                Amount = client.MonthlyAmount,
                DueDate = dueDate,
                Status = status,
                ReferenceYear = now.Year,
                ReferenceMonth = now.Month,
                CreatedAt = DateTime.UtcNow
            };

            _context.InvoiceRecords.Add(invoice);
            await _context.SaveChangesAsync();

            return new InvoiceResponseDto
            {
                Id = invoice.Id,
                ClientId = invoice.ClientId,
                ClientName = client.Name,
                Amount = invoice.Amount,
                DueDate = invoice.DueDate,
                Status = invoice.Status,
                PaidAt = invoice.PaidAt
            };
        }

        public async Task<PagedResultDto<InvoiceResponseDto>> GetAllAsync(Guid userId, InvoiceFilterRequestDto filter)
        {
            await UpdateOverdueInvoicesAsync(userId);

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize > 100 ? 100 : filter.PageSize;

            var query = _context.InvoiceRecords
                .AsNoTracking()
                .Include(i => i.Client)
                .Where(i => i.Client.UserId == userId);

            if (!string.IsNullOrWhiteSpace(filter.ClientName))
            {
                var clientName = filter.ClientName.Trim().ToLower();

                query = query.Where(i =>
                    i.Client.Name.ToLower().Contains(clientName));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(i => i.Status == filter.Status.Value);
            }

            if (filter.StartDate.HasValue)
            {
                var startDate = DateTime.SpecifyKind(filter.StartDate.Value, DateTimeKind.Utc);
                query = query.Where(i => i.DueDate >= startDate);
            }

            if (filter.EndDate.HasValue)
            {
                var endDate = DateTime.SpecifyKind(filter.EndDate.Value, DateTimeKind.Utc);
                query = query.Where(i => i.DueDate <= endDate);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(i => i.DueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new InvoiceResponseDto
                {
                    Id = i.Id,
                    ClientId = i.ClientId,
                    ClientName = i.Client.Name,
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    Status = i.Status,
                    PaidAt = i.PaidAt
                })
                .ToListAsync();

            return new PagedResultDto<InvoiceResponseDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<bool> MarkAsPaidAsync(Guid userId, Guid invoiceId)
        {
            var invoice = await _context.InvoiceRecords
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i =>
                    i.Id == invoiceId &&
                    i.Client.UserId == userId);

            if (invoice == null)
                return false;

            invoice.Status = PaymentStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        private async Task UpdateOverdueInvoicesAsync(Guid userId)
        {
            var overdueInvoices = await _context.InvoiceRecords
                .Include(i => i.Client)
                .Where(i =>
                    i.Client.UserId == userId &&
                    i.Status == PaymentStatus.Pending &&
                    i.DueDate < DateTime.UtcNow)
                .ToListAsync();

            foreach (var invoice in overdueInvoices)
            {
                invoice.Status = PaymentStatus.Overdue;
            }

            if (overdueInvoices.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
