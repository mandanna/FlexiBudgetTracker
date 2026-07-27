namespace ExpenseTracker.Api.Models
{
    public class Paycheck
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime ReceivedDate { get; set; }
        public bool IsClosed { get; set; }
        public List<Expense> Expenses { get; set; } = new();
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public List<Income> Incomes { get; set; } = new();
    }
}
