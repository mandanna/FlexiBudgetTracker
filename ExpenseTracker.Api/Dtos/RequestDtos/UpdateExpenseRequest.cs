namespace ExpenseTracker.Api.Dtos
{
    public class UpdateExpenseRequest
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public int? CategoryId { get; set; }
    }
}
