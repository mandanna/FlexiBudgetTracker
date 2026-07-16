namespace ExpenseTracker.Api.Dtos
{
    public class ApiResponse<T>
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public T? data { get; set; }
    }
}
