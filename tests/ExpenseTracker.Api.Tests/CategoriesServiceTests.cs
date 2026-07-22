using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Api.Tests;

public class CategoriesServiceTests
{
    [Fact]
    public async Task CreateCategoryAsync_WhenNameIsUnique_CreatesUserCategory()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new CategoriesService(context);

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
        var service = new CategoriesService(context);

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
        var service = new CategoriesService(context);

        var categories = await service.GetCategoriesAsync();

        Assert.Contains(categories, category => category.Name == "Food" && category.IsSystemCategory);
        Assert.Contains(categories, category => category.Name == "Rent" && category.IsSystemCategory);
    }
}
