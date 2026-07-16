namespace ExpenseTracker.Api.Dtos.RequestDtos.QueryRequest.Paycheck
{
    public class PaycheckQueryRequest: QueryRequest
    {
        public string? Search { get; set; }

        public bool? IsClosed { get; set; }
    }
}
