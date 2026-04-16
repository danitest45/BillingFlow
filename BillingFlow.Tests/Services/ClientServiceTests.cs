using BillingFlow.Application.DTOs.Clients;
using BillingFlow.Domain.Entities;
using BillingFlow.Infrastructure.Services;
using BillingFlow.Tests.Common;

namespace BillingFlow.Tests.Services;

public class ClientServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistClient()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new ClientService(context);
        var userId = Guid.NewGuid();

        var response = await service.CreateAsync(userId, new CreateClientRequestDto
        {
            Name = "Cliente",
            Email = "cliente@email.com",
            Phone = "1199999",
            MonthlyAmount = 100,
            DueDay = 10
        });

        Assert.Equal("Cliente", response.Name);
        Assert.Single(context.Clients);
    }

    [Fact]
    public async Task GetAllAsync_ShouldApplyFiltersAndPagination()
    {
        using var context = TestDbContextFactory.CreateContext();
        var userId = Guid.NewGuid();
        context.Clients.AddRange(
            new Client { Id = Guid.NewGuid(), UserId = userId, Name = "Alpha", Email = "a@e.com", MonthlyAmount = 100, DueDay = 10 },
            new Client { Id = Guid.NewGuid(), UserId = userId, Name = "Beta", Email = "b@e.com", MonthlyAmount = 200, DueDay = 15 },
            new Client { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Other", Email = "o@e.com", MonthlyAmount = 999, DueDay = 15 });
        await context.SaveChangesAsync();

        var service = new ClientService(context);

        var result = await service.GetAllAsync(userId, new ClientFilterRequestDto
        {
            Search = "be",
            DueDay = 15,
            MinMonthlyAmount = 150,
            MaxMonthlyAmount = 250,
            Page = 1,
            PageSize = 10
        });

        Assert.Single(result.Items);
        Assert.Equal("Beta", result.Items[0].Name);
        Assert.Equal(1, result.TotalCount);
    }

    [Theory]
    [InlineData(0, 0, 1, 10)]
    [InlineData(2, 500, 2, 100)]
    public async Task GetAllAsync_ShouldNormalizePageAndPageSize(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        using var context = TestDbContextFactory.CreateContext();
        var userId = Guid.NewGuid();
        context.Clients.Add(new Client { Id = Guid.NewGuid(), UserId = userId, Name = "One", Email = "1@e.com", MonthlyAmount = 100, DueDay = 10 });
        await context.SaveChangesAsync();

        var service = new ClientService(context);
        var result = await service.GetAllAsync(userId, new ClientFilterRequestDto { Page = page, PageSize = pageSize });

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedPageSize, result.PageSize);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenClientNotFound()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new ClientService(context);

        var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateClientRequestDto()));

        Assert.Equal("Cliente não encontrado.", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndReturnClient()
    {
        using var context = TestDbContextFactory.CreateContext();
        var userId = Guid.NewGuid();
        var client = new Client { Id = Guid.NewGuid(), UserId = userId, Name = "Old", Email = "old@e.com", Phone = "1", MonthlyAmount = 50, DueDay = 5 };
        context.Clients.Add(client);
        await context.SaveChangesAsync();

        var service = new ClientService(context);
        var response = await service.UpdateAsync(userId, client.Id, new UpdateClientRequestDto
        {
            Name = "New",
            Email = "new@e.com",
            Phone = "2",
            MonthlyAmount = 120,
            DueDay = 25
        });

        Assert.Equal("New", response.Name);
        Assert.Equal(120, response.MonthlyAmount);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenClientNotFound()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new ClientService(context);

        var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal("Cliente não encontrado.", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveClient()
    {
        using var context = TestDbContextFactory.CreateContext();
        var userId = Guid.NewGuid();
        var client = new Client { Id = Guid.NewGuid(), UserId = userId, Name = "Remover", Email = "r@e.com", MonthlyAmount = 100, DueDay = 10 };
        context.Clients.Add(client);
        await context.SaveChangesAsync();

        var service = new ClientService(context);
        await service.DeleteAsync(userId, client.Id);

        Assert.Empty(context.Clients);
    }
}
