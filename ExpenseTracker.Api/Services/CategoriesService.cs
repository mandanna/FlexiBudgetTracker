using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Interface;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services
{
    public class CategoriesService : ICategoriesService
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CategoriesService> _logger;
        public CategoriesService(ExpenseTrackerDbContext context, ICurrentUserService currentUserService, ILogger<CategoriesService> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }
        public async Task<CategoryResponse?> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == _currentUserService.UserId && c.Name.ToLower() == request.Name.ToLower());
            if (existingCategory == null) return null;
            var category = new Category
            {
                Name = request.Name,
                IsSystemCategory = false,
                UserId = _currentUserService.UserId
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {CategoryId} created.", category.Id);
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                IsSystemCategory = category.IsSystemCategory
            };

        }
        public async Task<List<CategoryResponse>> GetCategoriesAsync()
        {
            return await _context.Categories.Where(c => c.UserId == _currentUserService.UserId)
                .Select(x => new CategoryResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsSystemCategory = x.IsSystemCategory
                })
                .ToListAsync();
        }
    }
}
