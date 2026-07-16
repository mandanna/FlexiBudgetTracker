using ExpenseTracker.Api.Dtos;

namespace ExpenseTracker.Api.Interface
{
    public interface ICategoriesService
    {
        Task<CategoryResponse?> CreateCategoryAsync(CreateCategoryRequest request);
        Task<List<CategoryResponse>> GetCategoriesAsync();
    }
}
