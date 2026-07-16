using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Dtos
{
    public class CreateCategoryRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
