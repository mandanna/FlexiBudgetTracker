using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Interface
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
