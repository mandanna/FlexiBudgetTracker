using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Dtos.ResponseDtos.Category;
using ExpenseTracker.Api.Exceptions;
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
        private readonly ICacheService _cacheService;
        private static string UserCacheKey(int userId) => $"categories:user:{userId}";
        public CategoriesService(ExpenseTrackerDbContext context, ICurrentUserService currentUserService, ILogger<CategoriesService> logger, ICacheService cacheService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
            _cacheService = cacheService;
        }
        public async Task<CategoryResponse?> CreateCategoryAsync(CreateCategoryRequest request)
        {

            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == _currentUserService.UserId && c.Name.ToLower() == request.Name.ToLower());
            if (existingCategory != null) return null;
            var category = new Category
            {
                Name = request.Name,
                IsSystemCategory = false,
                UserId = _currentUserService.UserId
            };
            _context.Categories.Add(category);
            try
            {
                await _context.SaveChangesAsync();
                
            }
            catch (DbUpdateException)
            {
                throw new ConflictException("A category with this name already exists.");
            }
            await _cacheService.RemoveAsync(UserCacheKey(_currentUserService.UserId));
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
            var userId = _currentUserService.UserId;
            var systemcategories =await _cacheService.GetOrCreateAsync("categories:system",
               async () =>
                   await _context.Categories.Where(c => c.IsSystemCategory).Select(c => new CategoryResponse
                   {
                       Id = c.Id,
                       Name = c.Name,
                       IsSystemCategory = c.IsSystemCategory
                   }).ToListAsync());


            var userCategories=await _cacheService.GetOrCreateAsync(UserCacheKey(userId),
                async () => 
                await _context.Categories.Where(c => c.UserId == userId).Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsSystemCategory = c.IsSystemCategory

                }).ToListAsync()
                );

            return systemcategories.Concat(userCategories).ToList();

        }

        public async Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            // Only the user's own categories match — system categories (UserId == null)
            // never match this filter, so they're automatically protected from edits.
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == _currentUserService.UserId);
            if (category == null)
                throw new NotFoundException("Category not found.");

            category.Name = request.Name;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException("A category with this name already exists.");
            }
            await _cacheService.RemoveAsync(UserCacheKey(_currentUserService.UserId));
            _logger.LogInformation("Category {CategoryId} updated.", category.Id);
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                IsSystemCategory = category.IsSystemCategory
            };
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == _currentUserService.UserId);
            if (category == null)
                throw new NotFoundException("Category not found.");

            _context.Categories.Remove(category);   // expenses keep; their CategoryId → null (SetNull)
            await _context.SaveChangesAsync();
            await _cacheService.RemoveAsync(UserCacheKey(_currentUserService.UserId));

            _logger.LogInformation("Category {CategoryId} deleted.", id);
        }
    }
}
