namespace ExpenseTracker.Api.Dtos
{
    public class CreateIncomeRequest
    {
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ReceivedDate { get; set; }
    }
}
