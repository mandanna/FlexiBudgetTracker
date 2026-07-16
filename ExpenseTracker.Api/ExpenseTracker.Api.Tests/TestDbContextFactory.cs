using ExpenseTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Tests;

internal static class TestDbContextFactory
{
    public static ExpenseTrackerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ExpenseTrackerDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
