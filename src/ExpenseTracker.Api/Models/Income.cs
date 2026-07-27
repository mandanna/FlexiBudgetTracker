namespace ExpenseTracker.Api.Models
{
    public class Income
    {
        public int Id { get; set; }
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ReceivedDate { get; set; }
        public int PaycheckId { get; set; }
        public Paycheck Paycheck { get; set; } = null!;
    }
}
