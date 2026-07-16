namespace ExpenseTracker.Api.Dtos
{
    public class PaycheckSummaryResponse
    {
        public decimal PaycheckAmount { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal RemainingBalance { get; set; }
        public int ExpenseCount { get; set; }
        public bool IsClosed { get; set; }

    }
}
