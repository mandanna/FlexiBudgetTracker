using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Dtos
{
    public class UpdatePaycheckRequest
    {
        [Required]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ReceivedDate { get; set; }
    }
}
