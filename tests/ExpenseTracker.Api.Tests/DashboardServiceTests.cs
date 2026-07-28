using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Api.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_SettledFigures_CountOnlyClosedPaychecks_OpenIsProjectedSeparately()
    {
        using var context = TestDbContextFactory.CreateContext();

        // Closed A: saved 200 (income 1000, spent 800)
        var a = new Paycheck { Description = "A", ReceivedDate = new DateTime(2026, 6, 1), IsClosed = true, UserId = 1 };
        context.Incomes.Add(new Income { Amount = 1000m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 1), Paycheck = a });
        context.Expenses.Add(new Expense { Amount = 800m, Description = "s", ExpenseDate = new DateTime(2026, 6, 2), Paycheck = a });

        // Closed B: overspent 200 (income 1000, spent 1200)
        var b = new Paycheck { Description = "B", ReceivedDate = new DateTime(2026, 6, 15), IsClosed = true, UserId = 1 };
        context.Incomes.Add(new Income { Amount = 1000m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 15), Paycheck = b });
        context.Expenses.Add(new Expense { Amount = 1200m, Description = "s", ExpenseDate = new DateTime(2026, 6, 16), Paycheck = b });

        // Open C: projection 700 (income 1000, spent 300)
        var c = new Paycheck { Description = "C", ReceivedDate = new DateTime(2026, 7, 1), IsClosed = false, UserId = 1 };
        context.Incomes.Add(new Income { Amount = 1000m, Source = "Salary", ReceivedDate = new DateTime(2026, 7, 1), Paycheck = c });
        context.Expenses.Add(new Expense { Amount = 300m, Description = "s", ExpenseDate = new DateTime(2026, 7, 2), Paycheck = c });

        context.Paychecks.AddRange(a, b, c);
        await context.SaveChangesAsync();

        var service = new DashboardService(context, new FakeCurrentUserService(userId: 1));

        var result = await service.GetDashboardAsync(new DashboardQueryRequest());

        Assert.Equal(2000m, result.SettledIncome);
        Assert.Equal(2000m, result.SettledSpent);
        Assert.Equal(0m, result.SettledSavings);          // +200 and -200 net to 0
        Assert.Equal(2, result.SettledPaycheckCount);     // open C excluded from settled
        Assert.NotNull(result.CurrentPaycheck);
        Assert.Equal(c.Id, result.CurrentPaycheck!.Id);
        Assert.Equal(700m, result.CurrentPaycheck.ProjectedSavings);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenNoOpenPaycheck_CurrentPaycheckIsNull()
    {
        using var context = TestDbContextFactory.CreateContext();
        var p = new Paycheck { Description = "Closed", ReceivedDate = new DateTime(2026, 6, 1), IsClosed = true, UserId = 1 };
        context.Incomes.Add(new Income { Amount = 500m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 1), Paycheck = p });
        context.Paychecks.Add(p);
        await context.SaveChangesAsync();

        var service = new DashboardService(context, new FakeCurrentUserService(userId: 1));
        var result = await service.GetDashboardAsync(new DashboardQueryRequest());

        Assert.Null(result.CurrentPaycheck);
        Assert.Equal(500m, result.SettledIncome);
    }

    [Fact]
    public async Task GetDashboardAsync_DateRange_FiltersPaychecksByReceivedDate()
    {
        using var context = TestDbContextFactory.CreateContext();
        var inRange = new Paycheck { Description = "InRange", ReceivedDate = new DateTime(2026, 6, 15), IsClosed = true, UserId = 1 };
        context.Incomes.Add(new Income { Amount = 500m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 15), Paycheck = inRange });
        var outRange = new Paycheck { Description = "OutRange", ReceivedDate = new DateTime(2026, 5, 1), IsClosed = true, UserId = 1 };
        context.Incomes.Add(new Income { Amount = 900m, Source = "Salary", ReceivedDate = new DateTime(2026, 5, 1), Paycheck = outRange });
        context.Paychecks.AddRange(inRange, outRange);
        await context.SaveChangesAsync();

        var service = new DashboardService(context, new FakeCurrentUserService(userId: 1));
        var result = await service.GetDashboardAsync(new DashboardQueryRequest
        {
            FromDate = new DateTime(2026, 6, 1),
            ToDate = new DateTime(2026, 6, 30)
        });

        Assert.Equal(500m, result.SettledIncome);       // only the June paycheck
        Assert.Equal(1, result.SettledPaycheckCount);
    }

    [Fact]
    public async Task GetDashboardAsync_PaycheckIdFilter_ScopesToThatPaycheck()
    {
        using var context = TestDbContextFactory.CreateContext();
        var one = new Paycheck { Description = "One", ReceivedDate = new DateTime(2026, 6, 1), IsClosed = true, UserId = 1 };
        context.Incomes.Add(new Income { Amount = 500m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 1), Paycheck = one });
        var two = new Paycheck { Description = "Two", ReceivedDate = new DateTime(2026, 6, 15), IsClosed = true, UserId = 1 };
        context.Incomes.Add(new Income { Amount = 900m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 15), Paycheck = two });
        context.Paychecks.AddRange(one, two);
        await context.SaveChangesAsync();

        var service = new DashboardService(context, new FakeCurrentUserService(userId: 1));
        var result = await service.GetDashboardAsync(new DashboardQueryRequest { PaycheckId = one.Id });

        Assert.Equal(500m, result.SettledIncome);
        Assert.Equal(1, result.SettledPaycheckCount);
    }

    [Fact]
    public async Task GetDashboardAsync_SpendingByCategory_GroupsAndBucketsUncategorized()
    {
        using var context = TestDbContextFactory.CreateContext();
        var p = new Paycheck { Description = "P", ReceivedDate = new DateTime(2026, 6, 1), IsClosed = false, UserId = 1 };
        context.Incomes.Add(new Income { Amount = 1000m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 1), Paycheck = p });
        context.Expenses.AddRange(
            new Expense { Amount = 100m, Description = "Food1", ExpenseDate = new DateTime(2026, 6, 2), Paycheck = p, CategoryId = 1 },
            new Expense { Amount = 50m, Description = "Food2", ExpenseDate = new DateTime(2026, 6, 3), Paycheck = p, CategoryId = 1 },
            new Expense { Amount = 30m, Description = "NoCat", ExpenseDate = new DateTime(2026, 6, 4), Paycheck = p, CategoryId = null });
        context.Paychecks.Add(p);
        await context.SaveChangesAsync();

        var service = new DashboardService(context, new FakeCurrentUserService(userId: 1));
        var result = await service.GetDashboardAsync(new DashboardQueryRequest());

        var food = result.SpendingByCategory.Single(cat => cat.CategoryId == 1);
        Assert.Equal(150m, food.TotalAmount);
        Assert.Equal(2, food.ExpenseCount);

        var uncategorized = result.SpendingByCategory.Single(cat => cat.CategoryId == null);
        Assert.Equal(30m, uncategorized.TotalAmount);
        Assert.Equal("Uncategorized", uncategorized.CategoryName);
    }

    [Fact]
    public async Task GetDashboardAsync_DoesNotIncludeAnotherUsersData()
    {
        using var context = TestDbContextFactory.CreateContext();
        var other = new Paycheck { Description = "Other", ReceivedDate = new DateTime(2026, 6, 1), IsClosed = true, UserId = 2 };
        context.Incomes.Add(new Income { Amount = 999m, Source = "Salary", ReceivedDate = new DateTime(2026, 6, 1), Paycheck = other });
        context.Paychecks.Add(other);
        await context.SaveChangesAsync();

        var service = new DashboardService(context, new FakeCurrentUserService(userId: 1));
        var result = await service.GetDashboardAsync(new DashboardQueryRequest());

        Assert.Equal(0m, result.SettledIncome);
        Assert.Equal(0, result.SettledPaycheckCount);
        Assert.Null(result.CurrentPaycheck);
        Assert.Empty(result.SpendingByCategory);
        Assert.Empty(result.RecentExpenses);
    }
}