using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Dtos
{
    public class UpdateCategoryRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
