namespace ExpenseTracker.Api.Dtos
{
    public class PaycheckResponse
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public DateTime ReceivedDate { get; set; }
        public bool IsClosed { get; set; }
    }
}
