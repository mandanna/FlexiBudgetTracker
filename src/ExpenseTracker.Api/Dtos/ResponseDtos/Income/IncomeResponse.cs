namespace ExpenseTracker.Api.Dtos
{
    public class IncomeResponse
    {
        public int Id { get; set; }
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ReceivedDate { get; set; }
        public int PaycheckId { get; set; }
    }
}
