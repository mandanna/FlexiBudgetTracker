namespace ExpenseTracker.Api.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }=new string(string.Empty);

        public bool IsSystemCategory { get; set; }

        public List<Expense> Expenses { get; set; } = new();
        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
