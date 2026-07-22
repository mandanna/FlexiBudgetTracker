using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ExpenseTracker.Api.Tests;

public class CategoriesServiceTests
{
    [Fact]
    public async Task CreateCategoryAsync_WhenNameIsUnique_CreatesUserCategory()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new CategoriesService(context, new FakeCurrentUserService(userId: 1), NullLogger<CategoriesService>.Instance);

        var result = await service.CreateCategoryAsync(new CreateCategoryRequest
        {
            Name = "Subscriptions"
        });

        Assert.NotNull(result);
        Assert.Equal("Subscriptions", result.Name);
        Assert.False(result.IsSystemCategory);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenNameAlreadyExistsIgnoringCase_ReturnsNull()
    {
        using var context = TestDbContextFactory.CreateContext();
        // The dedup guard only checks the current user's own categories, so seed one
        // owned by user 1 — then a differently-cased "food" must be rejected as a duplicate.
        context.Categories.Add(new Category
        {
            Name = "Food",
            IsSystemCategory = false,
            UserId = 1
        });
        await context.SaveChangesAsync();

        var service = new CategoriesService(context, new FakeCurrentUserService(userId: 1), NullLogger<CategoriesService>.Instance);

        var result = await service.CreateCategoryAsync(new CreateCategoryRequest
        {
            Name = "food"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsSeededSystemCategories()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new CategoriesService(context, new FakeCurrentUserService(userId: 1), NullLogger<CategoriesService>.Instance);

        var categories = await service.GetCategoriesAsync();

        Assert.Contains(categories, category => category.Name == "Food" && category.IsSystemCategory);
        Assert.Contains(categories, category => category.Name == "Rent" && category.IsSystemCategory);
    }

    [Fact]
    public async Task GetCategoriesAsync_DoesNotReturnAnotherUsersCategories()
    {
        // Authorization boundary: user 1 sees system categories + their own, never user 2's.
        using var context = TestDbContextFactory.CreateContext();
        context.Categories.AddRange(
            new Category { Name = "MyCategory", IsSystemCategory = false, UserId = 1 },
            new Category { Name = "TheirCategory", IsSystemCategory = false, UserId = 2 });
        await context.SaveChangesAsync();

        var service = new CategoriesService(context, new FakeCurrentUserService(userId: 1), NullLogger<CategoriesService>.Instance);

        var categories = await service.GetCategoriesAsync();

        Assert.Contains(categories, c => c.Name == "MyCategory");
        Assert.DoesNotContain(categories, c => c.Name == "TheirCategory");
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenSystemCategory_ThrowsNotFoundException()
    {
        // System categories (UserId == null) never match the ownership filter, so they're
        // protected from edits — the service reports them as not found for the current user.
        using var context = TestDbContextFactory.CreateContext();
        var service = new CategoriesService(context, new FakeCurrentUserService(userId: 1), NullLogger<CategoriesService>.Instance);

        // Id 1 is the seeded system "Food" category.
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateCategoryAsync(1, new UpdateCategoryRequest { Name = "Hacked" }));
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenSystemCategory_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new CategoriesService(context, new FakeCurrentUserService(userId: 1), NullLogger<CategoriesService>.Instance);

        // Id 1 is the seeded system "Food" category.
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteCategoryAsync(1));
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenOwnedByUser_UpdatesName()
    {
        using var context = TestDbContextFactory.CreateContext();
        var category = new Category { Name = "Groceries", IsSystemCategory = false, UserId = 1 };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = new CategoriesService(context, new FakeCurrentUserService(userId: 1), NullLogger<CategoriesService>.Instance);

        var result = await service.UpdateCategoryAsync(category.Id, new UpdateCategoryRequest { Name = "Food & Drink" });

        Assert.Equal("Food & Drink", result.Name);
        Assert.Equal(category.Id, result.Id);
    }
}
