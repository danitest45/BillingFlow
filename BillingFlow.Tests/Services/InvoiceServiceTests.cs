using BillingFlow.Application.DTOs.Invoices;
using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using BillingFlow.Infrastructure.Services;
using BillingFlow.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace BillingFlow.Tests.Services;

public class InvoiceServiceTests
{
    [Fact]
    public async Task GenerateAsync_ShouldThrow_WhenClientNotFound()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new InvoiceService(context);

        var ex = await Assert.ThrowsAsync<Exception>(() => service.GenerateAsync(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal("Cliente não encontrado.", ex.Message);
    }

    [Fact]
    public async Task GenerateAsync_ShouldThrow_WhenInvoiceAlreadyExistsForMonth()
    {
        using var context = TestDbContextFactory.CreateContext();
        var userId = Guid.NewGuid();
        var client = new Client { Id = Guid.NewGuid(), UserId = userId, Name = "A", Email = "a@e.com", MonthlyAmount = 100, DueDay = 10 };
        var now = DateTime.UtcNow;
        context.Clients.Add(client);
        context.InvoiceRecords.Add(new InvoiceRecord
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            Amount = 100,
            DueDate = now,
            Status = PaymentStatus.Pending,
            ReferenceYear = now.Year,
            ReferenceMonth = now.Month
        });
        await context.SaveChangesAsync();

        var service = new InvoiceService(context);
        var ex = await Assert.ThrowsAsync<Exception>(() => service.GenerateAsync(userId, client.Id));

        Assert.Equal("Já existe cobrança para este cliente no mês atual.", ex.Message);
    }

    [Fact]
    public async Task GenerateAsync_ShouldCreateInvoice_WithValidFields()
    {
        using var context = TestDbContextFactory.CreateContext();
        var userId = Guid.NewGuid();
        var client = new Client { Id = Guid.NewGuid(), UserId = userId, Name = "A", Email = "a@e.com", MonthlyAmount = 150, DueDay = 31 };
        context.Clients.Add(client);
        await context.SaveChangesAsync();

        var service = new InvoiceService(context);
        var result = await service.GenerateAsync(userId, client.Id);

        Assert.Equal(client.Id, result.ClientId);
        Assert.Equal(150, result.Amount);
        Assert.True(result.Status is PaymentStatus.Pending or PaymentStatus.Overdue);
        Assert.Single(context.InvoiceRecords);
    }

    [Fact]
    public async Task GetAllAsync_ShouldUpdateOverdueAndApplyFilters()
    {
        using var context = TestDbContextFactory.CreateContext();
        var userId = Guid.NewGuid();
        var client = new Client { Id = Guid.NewGuid(), UserId = userId, Name = "Cliente Busca", Email = "c@e.com", MonthlyAmount = 100, DueDay = 10 };
        context.Clients.Add(client);

        var overdueDueDate = DateTime.UtcNow.AddDays(-2);
        var futureDueDate = DateTime.UtcNow.AddDays(5);

        context.InvoiceRecords.AddRange(
            new InvoiceRecord { Id = Guid.NewGuid(), ClientId = client.Id, Amount = 100, DueDate = overdueDueDate, Status = PaymentStatus.Pending, ReferenceYear = overdueDueDate.Year, ReferenceMonth = overdueDueDate.Month },
            new InvoiceRecord { Id = Guid.NewGuid(), ClientId = client.Id, Amount = 200, DueDate = futureDueDate, Status = PaymentStatus.Pending, ReferenceYear = futureDueDate.Year, ReferenceMonth = futureDueDate.Month });

        await context.SaveChangesAsync();

        var service = new InvoiceService(context);
        var result = await service.GetAllAsync(userId, new InvoiceFilterRequestDto
        {
            ClientName = "busca",
            Status = PaymentStatus.Overdue,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(1),
            Page = 1,
            PageSize = 10
        });

        Assert.Single(result.Items);
        Assert.Equal(PaymentStatus.Overdue, result.Items[0].Status);

        var updated = await context.InvoiceRecords.FirstAsync(x => x.DueDate == overdueDueDate);
        Assert.Equal(PaymentStatus.Overdue, updated.Status);
    }

    [Fact]
    public async Task MarkAsPaidAsync_ShouldReturnFalse_WhenInvoiceNotFound()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new InvoiceService(context);

        var success = await service.MarkAsPaidAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(success);
    }

    [Fact]
    public async Task MarkAsPaidAsync_ShouldMarkInvoiceAsPaid()
    {
        using var context = TestDbContextFactory.CreateContext();
        var userId = Guid.NewGuid();
        var client = new Client { Id = Guid.NewGuid(), UserId = userId, Name = "A", Email = "a@e.com", MonthlyAmount = 100, DueDay = 10 };
        var invoice = new InvoiceRecord
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            Amount = 100,
            DueDate = DateTime.UtcNow,
            Status = PaymentStatus.Pending,
            ReferenceYear = DateTime.UtcNow.Year,
            ReferenceMonth = DateTime.UtcNow.Month
        };

        context.Clients.Add(client);
        context.InvoiceRecords.Add(invoice);
        await context.SaveChangesAsync();

        var service = new InvoiceService(context);
        var success = await service.MarkAsPaidAsync(userId, invoice.Id);

        Assert.True(success);
        var updated = await context.InvoiceRecords.FirstAsync();
        Assert.Equal(PaymentStatus.Paid, updated.Status);
        Assert.NotNull(updated.PaidAt);
    }
}
