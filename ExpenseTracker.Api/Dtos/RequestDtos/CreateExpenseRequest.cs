using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Dtos
{
    public class CreateExpenseRequest
    {

        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }
        public int? CategoryId { get; set; }

    }
}
