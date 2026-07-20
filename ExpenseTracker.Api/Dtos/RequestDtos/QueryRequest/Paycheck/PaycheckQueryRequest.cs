namespace ExpenseTracker.Api.Dtos
{
    public class PaycheckQueryRequest: QueryRequest
    {
        public string? Search { get; set; }

        public bool? IsClosed { get; set; }
    }
}
