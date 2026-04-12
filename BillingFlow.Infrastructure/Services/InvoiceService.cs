using BillingFlow.Application.DTOs.Invoices;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Enums;
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
            var client = await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.Id == clientId &&
                    x.UserId == userId);

            if (client is null)
                throw new Exception("Cliente não encontrado.");

            var dueDate = DateTime.SpecifyKind(
                new DateTime(
                    DateTime.UtcNow.Year,
                    DateTime.UtcNow.Month,
                    client.DueDay),
                DateTimeKind.Utc);

            var invoice = new Domain.Entities.InvoiceRecord
            {
                Id = Guid.NewGuid(),
                ClientId = client.Id,
                Amount = client.MonthlyAmount,
                DueDate = dueDate,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.InvoiceRecords.Add(invoice);

            await _context.SaveChangesAsync();

            return new InvoiceResponseDto
            {
                Id = invoice.Id,
                ClientId = invoice.ClientId,
                Amount = invoice.Amount,
                DueDate = invoice.DueDate,
                Status = invoice.Status
            };
        }

        public async Task<List<InvoiceResponseDto>> GetAllAsync(Guid userId)
        {
            return await _context.InvoiceRecords
                .Where(x => x.Client.UserId == userId)
                .Select(x => new InvoiceResponseDto
                {
                    Id = x.Id,
                    ClientId = x.ClientId,
                    Amount = x.Amount,
                    DueDate = x.DueDate,
                    Status = x.Status
                })
                .ToListAsync();
        }
    }
}
