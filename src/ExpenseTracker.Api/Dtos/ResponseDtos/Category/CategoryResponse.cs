namespace ExpenseTracker.Api.Dtos.ResponseDtos.Category
{
    public class CategoryResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsSystemCategory { get; set; }
    }
}
