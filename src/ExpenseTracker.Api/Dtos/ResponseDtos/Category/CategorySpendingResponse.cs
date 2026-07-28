namespace ExpenseTracker.Api.Dtos.ResponseDtos.Category
{
    public class CategorySpendingResponse
    {
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = "Uncategorized";
        public decimal? TotalAmount { get; set; }
        public int ExpenseCount { get; set; }
    }
}
