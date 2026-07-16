namespace ExpenseTracker.Api.Dtos.ResponseDtos
{
    public class PaycheckDetailsResponse
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;

        public DateTime ReceivedDate { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal RemainingBalance { get; set; }
        public int ExpenseCount { get; set; }
        public bool IsClosed { get; set; }
        public int UserId { get; set; }
        public List<ExpenseResponse> Expenses { get; set; } = new();
    }
}
