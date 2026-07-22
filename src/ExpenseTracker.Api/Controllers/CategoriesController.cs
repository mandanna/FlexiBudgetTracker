
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Interface;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesService _categoriesService;
        public CategoriesController(ICategoriesService categoriesService) => _categoriesService = categoriesService;

        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
        {
            var category = await _categoriesService.CreateCategoryAsync(request);
            if (category == null) return BadRequest(new ApiResponse<string>
            {
                success = false,
                message = "Category already exists",
                data = null
            });
            return Ok(new ApiResponse<CategoryResponse>
            {
                success = true,
                message = "Category created successfully",
                data = category
            });
        }
        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetAllSystemCategories()
        {

            var categories = await _categoriesService.GetCategoriesAsync();
            if (categories == null)
            {
                return NotFound(new ApiResponse<List<CategoryResponse>>
                {
                    data = null,
                    message = "No categories found.",
                    success = false
                });
            }
            return Ok(new ApiResponse<List<CategoryResponse>>
            {
                success = true,
                message = "Categories retrieved successfully",
                data = categories
            });
        }

        [HttpPut("UpdateCategory")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryRequest request)
        {
            var updated = await _categoriesService.UpdateCategoryAsync(id, request);
            return Ok(new ApiResponse<CategoryResponse>
            {
                success = true,
                message = "Category updated successfully",
                data = updated
            });
        }

        [HttpDelete("DeleteCategory")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoriesService.DeleteCategoryAsync(id);
            return Ok(new ApiResponse<string>
            {
                success = true,
                message = "Category deleted successfully"
            });
        }
    }
}
