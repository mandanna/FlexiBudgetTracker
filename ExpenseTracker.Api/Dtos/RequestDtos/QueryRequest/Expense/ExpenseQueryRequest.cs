namespace ExpenseTracker.Api.Dtos.RequestDtos.QueryRequest.Expense
{
    public class ExpenseQueryRequest : QueryRequest
    {
        public int? CategoryId { get; set; }

        public int? PaycheckId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
