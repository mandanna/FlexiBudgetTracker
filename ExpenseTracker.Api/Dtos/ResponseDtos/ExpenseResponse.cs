namespace ExpenseTracker.Api.Dtos
{
    public class ExpenseResponse
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public int PaycheckId { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}