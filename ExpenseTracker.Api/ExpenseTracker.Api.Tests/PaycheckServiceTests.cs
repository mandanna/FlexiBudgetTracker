using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Api.Tests;

public class PaycheckServiceTests
{
    [Fact]
    public async Task CreatePaycheckAsync_CreatesOpenPaycheck()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new PaycheckService(context);

        var result = await service.CreatePaycheckAsync(new CreatePaycheckRequest
        {
            Amount = 1500m,
            Description = "June paycheck",
            ReceivedDate = new DateTime(2026, 6, 15)
        });

        Assert.True(result.Id > 0);
        Assert.Equal(1500m, result.Amount);
        Assert.Equal("June paycheck", result.Description);
        Assert.False(result.IsClosed);
    }

    [Fact]
    public async Task ClosePaycheck_WhenPaycheckExists_UpdatesClosedStatus()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Amount = 900m,
            Description = "Paycheck",
            ReceivedDate = new DateTime(2026, 6, 1)
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new PaycheckService(context);

        var result = await service.ClosePaycheck(paycheck.Id, true);

        Assert.True(result);
        Assert.True(context.Paychecks.Single(x => x.Id == paycheck.Id).IsClosed);
    }

    [Fact]
    public async Task ClosePaycheck_WhenPaycheckDoesNotExist_ReturnsFalse()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new PaycheckService(context);

        var result = await service.ClosePaycheck(404, true);

        Assert.False(result);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenPaycheckExists_ReturnsTotals()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Amount = 1000m,
            Description = "Paycheck",
            ReceivedDate = new DateTime(2026, 6, 1)
        };
        context.Paychecks.Add(paycheck);
        context.Expenses.AddRange(
            new Expense
            {
                Amount = 100m,
                Description = "Groceries",
                ExpenseDate = new DateTime(2026, 6, 2),
                Paycheck = paycheck
            },
            new Expense
            {
                Amount = 250m,
                Description = "Rent",
                ExpenseDate = new DateTime(2026, 6, 3),
                Paycheck = paycheck
            });
        await context.SaveChangesAsync();

        var service = new PaycheckService(context);

        var summary = await service.GetSummaryAsync(paycheck.Id);

        Assert.NotNull(summary);
        Assert.Equal(1000m, summary.PaycheckAmount);
        Assert.Equal(350m, summary.TotalExpenses);
        Assert.Equal(650m, summary.RemainingBalance);
        Assert.Equal(2, summary.ExpenseCount);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenPaycheckDoesNotExist_ReturnsNull()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new PaycheckService(context);

        var summary = await service.GetSummaryAsync(404);

        Assert.Null(summary);
    }
}
