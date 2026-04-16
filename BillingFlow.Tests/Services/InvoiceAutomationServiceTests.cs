using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using BillingFlow.Infrastructure.Services;
using BillingFlow.Tests.Common;

namespace BillingFlow.Tests.Services;

public class InvoiceAutomationServiceTests
{
    [Fact]
    public async Task GenerateMissingInvoicesForCurrentMonthAsync_ShouldReturnZero_WhenNoClients()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new InvoiceAutomationService(context);

        var created = await service.GenerateMissingInvoicesForCurrentMonthAsync();

        Assert.Equal(0, created);
        Assert.Empty(context.InvoiceRecords);
    }

    [Fact]
    public async Task GenerateMissingInvoicesForCurrentMonthAsync_ShouldCreateOnlyMissingInvoices()
    {
        using var context = TestDbContextFactory.CreateContext();
        var now = DateTime.UtcNow;
        var activeClientWithInvoice = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "A", Email = "a@e.com", MonthlyAmount = 100, DueDay = 10, IsActive = true };
        var activeClientWithoutInvoice = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "B", Email = "b@e.com", MonthlyAmount = 200, DueDay = 31, IsActive = true };
        var inactiveClient = new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "C", Email = "c@e.com", MonthlyAmount = 300, DueDay = 20, IsActive = false };

        context.Clients.AddRange(activeClientWithInvoice, activeClientWithoutInvoice, inactiveClient);
        context.InvoiceRecords.Add(new InvoiceRecord
        {
            Id = Guid.NewGuid(),
            ClientId = activeClientWithInvoice.Id,
            Amount = 100,
            DueDate = DateTime.UtcNow,
            Status = PaymentStatus.Pending,
            ReferenceYear = now.Year,
            ReferenceMonth = now.Month
        });
        await context.SaveChangesAsync();

        var service = new InvoiceAutomationService(context);
        var created = await service.GenerateMissingInvoicesForCurrentMonthAsync();

        Assert.Equal(1, created);

        var createdInvoice = context.InvoiceRecords.Single(x => x.ClientId == activeClientWithoutInvoice.Id);
        Assert.Equal(activeClientWithoutInvoice.MonthlyAmount, createdInvoice.Amount);
        Assert.True(createdInvoice.Status is PaymentStatus.Pending or PaymentStatus.Overdue);
    }
}
