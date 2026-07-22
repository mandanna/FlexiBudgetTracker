namespace ExpenseTracker.Api.Interface
{
    public interface IPasswordHasherService
    {
        string HashPassword(string password);

        bool VerifyPassword(string password, string passwordHash);
    }
}
