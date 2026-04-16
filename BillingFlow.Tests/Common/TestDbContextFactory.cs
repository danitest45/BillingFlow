using BillingFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BillingFlow.Tests.Common;

internal static class TestDbContextFactory
{
    public static BillingFlowDbContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<BillingFlowDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new BillingFlowDbContext(options);
    }
}
