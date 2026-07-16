namespace ExpenseTracker.Api.Dtos.RequestDtos.QueryRequest
{
    public class QueryRequest
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }

        public bool Descending { get; set; }
    }
}
