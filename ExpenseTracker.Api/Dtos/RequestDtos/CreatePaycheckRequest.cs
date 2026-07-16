using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Dtos
{
    public class CreatePaycheckRequest
    {
        [Required]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Range(0.01,double.MaxValue)]
        public decimal Amount { get; set; }
        [Required]
        public DateTime ReceivedDate { get; set; }
    }
}
