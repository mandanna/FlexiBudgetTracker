using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ExpenseTracker.Api.Tests;

public class PaycheckServiceTests
{
    [Fact]
    public async Task CreatePaycheckAsync_CreatesOpenPaycheck()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        var result = await service.CreatePaycheckAsync(new CreatePaycheckRequest
        {
            Description = "June paycheck",
            ReceivedDate = new DateTime(2026, 6, 15)
        });

        Assert.True(result.Id > 0);
        Assert.Equal(0m, result.TotalIncome); // a new period has no income until incomes are added
        Assert.Equal("June paycheck", result.Description);
        Assert.False(result.IsClosed);
    }

    [Fact]
    public async Task ClosePaycheck_WhenPaycheckExists_UpdatesClosedStatus()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Paycheck",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 1
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        var result = await service.ClosePaycheck(paycheck.Id, true);

        Assert.True(result);
        Assert.True(context.Paychecks.Single(x => x.Id == paycheck.Id).IsClosed);
    }

    [Fact]
    public async Task ClosePaycheck_WhenReopening_SetsIsClosedFalse()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Closed paycheck",
            ReceivedDate = new DateTime(2026, 6, 1),
            IsClosed = true,
            UserId = 1
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        var result = await service.ClosePaycheck(paycheck.Id, false);

        Assert.True(result);
        Assert.False(context.Paychecks.Single(x => x.Id == paycheck.Id).IsClosed);
    }

    [Fact]
    public async Task ClosePaycheck_WhenPaycheckDoesNotExist_ReturnsFalse()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        var result = await service.ClosePaycheck(404, true);

        Assert.False(result);
    }

    [Fact]
    public async Task ClosePaycheck_WhenPaycheckBelongsToAnotherUser_ReturnsFalse()
    {
        // Authorization boundary: user 1 cannot close user 2's paycheck.
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Someone else's paycheck",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 2
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        var result = await service.ClosePaycheck(paycheck.Id, true);

        Assert.False(result);
        Assert.False(context.Paychecks.Single(x => x.Id == paycheck.Id).IsClosed);
    }

    [Fact]
    public async Task GetPaycheckAsync_WhenExists_ReturnsPaycheckWithTotalIncome()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "July paycheck",
            ReceivedDate = new DateTime(2026, 7, 1),
            UserId = 1
        };
        context.Incomes.Add(new Income { Amount = 1200m, Source = "Salary", ReceivedDate = new DateTime(2026, 7, 1), Paycheck = paycheck });
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        var result = await service.GetPaycheckAsync(paycheck.Id);

        Assert.NotNull(result);
        Assert.Equal(paycheck.Id, result.Id);
        Assert.Equal(1200m, result.TotalIncome); // derived from the income row, not a stored Amount
        Assert.Equal("July paycheck", result.Description);
    }

    [Fact]
    public async Task GetPaycheckAsync_WhenBelongsToAnotherUser_ThrowsNotFoundException()
    {
        // Authorization boundary: user 1 cannot read user 2's paycheck.
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Someone else's paycheck",
            ReceivedDate = new DateTime(2026, 7, 1),
            UserId = 2
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetPaycheckAsync(paycheck.Id));
    }

    [Fact]
    public async Task GetSummaryAsync_WhenPaycheckExists_ReturnsTotals()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Paycheck",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 1
        };
        // Two incomes in the same period, summing to 1000 — exercises multi-income totalling.
        context.Incomes.AddRange(
            new Income { Amount = 600m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 1), Paycheck = paycheck },
            new Income { Amount = 400m, Source = "Freelance", ReceivedDate = new DateTime(2026, 6, 2), Paycheck = paycheck });
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

        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        var summary = await service.GetSummaryAsync(paycheck.Id);

        Assert.NotNull(summary);
        Assert.Equal(1000m, summary.PaycheckAmount); // total income
        Assert.Equal(350m, summary.TotalExpenses);
        Assert.Equal(650m, summary.RemainingBalance);
        Assert.Equal(2, summary.ExpenseCount);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenPaycheckDoesNotExist_ReturnsNull()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        var summary = await service.GetSummaryAsync(404);

        Assert.Null(summary);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenPaycheckBelongsToAnotherUser_ReturnsNull()
    {
        // Authorization boundary: a summary for another user's paycheck must not leak.
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Someone else's paycheck",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 2
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        var summary = await service.GetSummaryAsync(paycheck.Id);

        Assert.Null(summary);
    }

    [Fact]
    public async Task UpdatePaycheckAsync_WhenOpen_UpdatesFields()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Original",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 1
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        var result = await service.UpdatePaycheckAsync(paycheck.Id, new UpdatePaycheckRequest
        {
            Description = "Revised",
            ReceivedDate = new DateTime(2026, 6, 5)
        });

        Assert.Equal("Revised", result.Description);
        Assert.Equal(new DateTime(2026, 6, 5), result.ReceivedDate);
    }

    [Fact]
    public async Task UpdatePaycheckAsync_WhenClosed_ThrowsBusinessRuleException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Closed",
            ReceivedDate = new DateTime(2026, 6, 1),
            IsClosed = true,
            UserId = 1
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdatePaycheckAsync(paycheck.Id, new UpdatePaycheckRequest
            {
                Description = "Revised",
                ReceivedDate = new DateTime(2026, 6, 5)
            }));
    }

    [Fact]
    public async Task DeletePaycheckAsync_WhenExists_RemovesPaycheck()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Delete me",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 1
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        await service.DeletePaycheckAsync(paycheck.Id);

        Assert.Empty(context.Paychecks.Where(x => x.Id == paycheck.Id));
    }

    [Fact]
    public async Task DeletePaycheckAsync_WhenNotFound_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new PaycheckService(context, new FakeCurrentUserService(userId: 1), NullLogger<PaycheckService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeletePaycheckAsync(404));
    }
}
