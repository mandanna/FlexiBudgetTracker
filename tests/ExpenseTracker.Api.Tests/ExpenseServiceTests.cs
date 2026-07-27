using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ExpenseTracker.Api.Tests;

public class ExpenseServiceTests
{
    [Fact]
    public async Task CreateExpenseAsync_WhenPaycheckHasEnoughRemainingBalance_CreatesExpense()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "June first half",
            ReceivedDate = new DateTime(2026, 6, 15),
            UserId = 1
        };
        context.Incomes.Add(new Income { Amount = 1000m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 15), Paycheck = paycheck });
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, new FakeCurrentUserService(userId: 1), NullLogger<ExpenseService>.Instance);

        var result = await service.CreateExpenseAsync(paycheck.Id, new CreateExpenseRequest
        {
            Amount = 125.50m,
            Description = "Groceries",
            ExpenseDate = new DateTime(2026, 6, 16),
            CategoryId = 1
        });

        Assert.NotNull(result);
        Assert.Equal(125.50m, result.Amount);
        Assert.Equal("Groceries", result.Description);
        Assert.Equal(paycheck.Id, result.PaycheckId);
    }

    [Fact]
    public async Task CreateExpenseAsync_WhenPaycheckIsClosed_ThrowsBusinessRuleException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Closed check",
            ReceivedDate = new DateTime(2026, 6, 1),
            IsClosed = true,
            UserId = 1
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, new FakeCurrentUserService(userId: 1), NullLogger<ExpenseService>.Instance);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateExpenseAsync(paycheck.Id, new CreateExpenseRequest
            {
                Amount = 10m,
                Description = "Coffee",
                ExpenseDate = new DateTime(2026, 6, 2)
            }));
    }

    [Fact]
    public async Task CreateExpenseAsync_WhenExpenseExceedsRemainingBalance_ThrowsBusinessRuleException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Small check",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 1
        };
        context.Incomes.Add(new Income { Amount = 100m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 1), Paycheck = paycheck });
        context.Paychecks.Add(paycheck);
        context.Expenses.Add(new Expense
        {
            Amount = 75m,
            Description = "Existing expense",
            ExpenseDate = new DateTime(2026, 6, 2),
            Paycheck = paycheck
        });
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, new FakeCurrentUserService(userId: 1), NullLogger<ExpenseService>.Instance);

        // income 100 - already-spent 75 = 25 left, so 30 must be rejected.
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateExpenseAsync(paycheck.Id, new CreateExpenseRequest
            {
                Amount = 30m,
                Description = "Too much",
                ExpenseDate = new DateTime(2026, 6, 3)
            }));
    }

    [Fact]
    public async Task CreateExpenseAsync_WhenPaycheckBelongsToAnotherUser_ThrowsNotFoundException()
    {
        // Authorization boundary: user 1 must not be able to add an expense to user 2's paycheck.
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Someone else's paycheck",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 2
        };
        context.Paychecks.Add(paycheck);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, new FakeCurrentUserService(userId: 1), NullLogger<ExpenseService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateExpenseAsync(paycheck.Id, new CreateExpenseRequest
            {
                Amount = 10m,
                Description = "Sneaky",
                ExpenseDate = new DateTime(2026, 6, 2)
            }));
    }

    [Fact]
    public async Task UpdateExpenseAsync_WhenExpenseBelongsToOpenPaycheck_UpdatesExpense()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Paycheck",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 1
        };
        var expense = new Expense
        {
            Amount = 40m,
            Description = "Original",
            ExpenseDate = new DateTime(2026, 6, 2),
            Paycheck = paycheck,
            CategoryId = 1
        };
        context.Incomes.Add(new Income { Amount = 400m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 1), Paycheck = paycheck });
        context.AddRange(paycheck, expense);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, new FakeCurrentUserService(userId: 1), NullLogger<ExpenseService>.Instance);

        var result = await service.UpdateExpenseAsync(paycheck.Id, expense.Id, new UpdateExpenseRequest
        {
            Amount = 55m,
            Description = "Updated",
            ExpenseDate = new DateTime(2026, 6, 3),
            CategoryId = 2
        });

        Assert.NotNull(result);
        Assert.Equal(55m, result.Amount);
        Assert.Equal("Updated", result.Description);
        Assert.Equal(new DateTime(2026, 6, 3), result.ExpenseDate);
    }

    [Fact]
    public async Task UpdateExpenseAsync_WhenPaycheckIsClosed_ThrowsBusinessRuleException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Closed paycheck",
            ReceivedDate = new DateTime(2026, 6, 1),
            IsClosed = true,
            UserId = 1
        };
        var expense = new Expense
        {
            Amount = 40m,
            Description = "Original",
            ExpenseDate = new DateTime(2026, 6, 2),
            Paycheck = paycheck
        };
        context.AddRange(paycheck, expense);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, new FakeCurrentUserService(userId: 1), NullLogger<ExpenseService>.Instance);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateExpenseAsync(paycheck.Id, expense.Id, new UpdateExpenseRequest
            {
                Amount = 55m,
                Description = "Updated",
                ExpenseDate = new DateTime(2026, 6, 3)
            }));
    }

    [Fact]
    public async Task GetExpenseAsync_WhenExpenseBelongsToAnotherUser_ReturnsNull()
    {
        // Authorization boundary: an expense reached through another user's paycheck is invisible.
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Someone else's paycheck",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 2
        };
        var expense = new Expense
        {
            Amount = 25m,
            Description = "Their expense",
            ExpenseDate = new DateTime(2026, 6, 2),
            Paycheck = paycheck
        };
        context.AddRange(paycheck, expense);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, new FakeCurrentUserService(userId: 1), NullLogger<ExpenseService>.Instance);

        var result = await service.GetExpenseAsync(paycheck.Id, expense.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveExpenseAsync_WhenExpenseBelongsToPaycheck_RemovesExpense()
    {
        using var context = TestDbContextFactory.CreateContext();
        var paycheck = new Paycheck
        {
            Description = "Paycheck",
            ReceivedDate = new DateTime(2026, 6, 1),
            UserId = 1
        };
        var expense = new Expense
        {
            Amount = 25m,
            Description = "Delete me",
            ExpenseDate = new DateTime(2026, 6, 2),
            Paycheck = paycheck
        };
        context.AddRange(paycheck, expense);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, new FakeCurrentUserService(userId: 1), NullLogger<ExpenseService>.Instance);

        var removed = await service.RemoveExpenseAsync(paycheck.Id, expense.Id);

        Assert.True(removed);
        Assert.Empty(context.Expenses.Where(x => x.Id == expense.Id));
    }

    [Fact]
    public async Task RemoveExpenseAsync_WhenExpenseDoesNotExist_ReturnsFalse()
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

        var service = new ExpenseService(context, new FakeCurrentUserService(userId: 1), NullLogger<ExpenseService>.Instance);

        var removed = await service.RemoveExpenseAsync(paycheck.Id, expenseId: 404);

        Assert.False(removed);
    }
}
