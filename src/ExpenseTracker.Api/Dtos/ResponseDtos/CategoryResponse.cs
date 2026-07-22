namespace ExpenseTracker.Api.Dtos
{
    public class CategoryResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsSystemCategory { get; set; }
    }
}
