namespace ExpenseTracker.Api.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public int PaycheckId { get; set; }
        public Paycheck Paycheck { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
