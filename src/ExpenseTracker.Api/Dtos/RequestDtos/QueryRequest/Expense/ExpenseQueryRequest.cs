namespace ExpenseTracker.Api.Dtos
{
    public class ExpenseQueryRequest : QueryRequest
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }

        public int? PaycheckId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
