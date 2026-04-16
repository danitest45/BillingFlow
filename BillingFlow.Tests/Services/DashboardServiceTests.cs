using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using BillingFlow.Infrastructure.Services;
using BillingFlow.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace BillingFlow.Tests.Services;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ShouldUpdateOverdueAndCalculateTotals()
    {
        using var context = TestDbContextFactory.CreateContext();
        var userId = Guid.NewGuid();
        var client = new Client { Id = Guid.NewGuid(), UserId = userId, Name = "Cliente", Email = "c@e.com", DueDay = 10, MonthlyAmount = 100 };
        context.Clients.Add(client);

        context.InvoiceRecords.AddRange(
            new InvoiceRecord { Id = Guid.NewGuid(), ClientId = client.Id, Amount = 100, DueDate = DateTime.UtcNow.AddDays(-1), Status = PaymentStatus.Pending, ReferenceYear = 2026, ReferenceMonth = 1 },
            new InvoiceRecord { Id = Guid.NewGuid(), ClientId = client.Id, Amount = 200, DueDate = DateTime.UtcNow, Status = PaymentStatus.Paid, PaidAt = DateTime.UtcNow, ReferenceYear = 2026, ReferenceMonth = 1 },
            new InvoiceRecord { Id = Guid.NewGuid(), ClientId = client.Id, Amount = 300, DueDate = DateTime.UtcNow.AddDays(2), Status = PaymentStatus.Pending, ReferenceYear = 2026, ReferenceMonth = 1 });

        await context.SaveChangesAsync();

        var service = new DashboardService(context);
        var result = await service.GetSummaryAsync(userId);

        Assert.Equal(600, result.TotalExpected);
        Assert.Equal(200, result.TotalPaid);
        Assert.Equal(300, result.TotalPending);
        Assert.Equal(1, result.OverdueCount);

        var overdueInvoice = await context.InvoiceRecords.FirstAsync(i => i.Amount == 100);
        Assert.Equal(PaymentStatus.Overdue, overdueInvoice.Status);
    }
}
