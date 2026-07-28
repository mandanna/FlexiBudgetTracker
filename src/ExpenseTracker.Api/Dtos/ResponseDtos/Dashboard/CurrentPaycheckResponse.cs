namespace ExpenseTracker.Api.Dtos
{
    public class CurrentPaycheckResponse
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal ProjectedSavings { get; set; }
        public int ExpenseCount { get; set; }
    }
}
