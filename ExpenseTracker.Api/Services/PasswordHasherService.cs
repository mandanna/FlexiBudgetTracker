using ExpenseTracker.Api.Interface;
using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Api.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly PasswordHasher<User> _passwordHasher = new();

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null!, password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(
          null!,
          passwordHash,
          password);
            return result != PasswordVerificationResult.Failed;
        }
    }
}
