namespace ExpenseTracker.Api.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public List<Paycheck> Paychecks { get; set; } = new();

        public List<Category> Categories { get; set; } = new();
    }
}
