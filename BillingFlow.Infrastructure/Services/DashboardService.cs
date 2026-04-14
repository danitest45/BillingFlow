using BillingFlow.Application.DTOs.Dashboard;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Enums;
using BillingFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly BillingFlowDbContext _context;

        public DashboardService(BillingFlowDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(Guid userId)
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

            var invoices = await _context.InvoiceRecords
                .Include(i => i.Client)
                .Where(i => i.Client.UserId == userId)
                .ToListAsync();

            return new DashboardSummaryDto
            {
                TotalExpected = invoices.Sum(i => i.Amount),

                TotalPaid = invoices
                    .Where(i => i.Status == PaymentStatus.Paid)
                    .Sum(i => i.Amount),

                TotalPending = invoices
                    .Where(i => i.Status == PaymentStatus.Pending)
                    .Sum(i => i.Amount),

                OverdueCount = invoices
                    .Count(i => i.Status == PaymentStatus.Overdue)
            };
        }
    }
}
