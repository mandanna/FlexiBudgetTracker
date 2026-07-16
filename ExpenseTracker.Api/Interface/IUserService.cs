using ExpenseTracker.Api.Dtos;
using Microsoft.AspNetCore.Identity.Data;

namespace ExpenseTracker.Api.Interface
{
    public interface IUserService
    {
        Task<UserResponse?> RegisterAsync(UserRegisterRequest request);
        Task<LoginResponse?> LoginAsync(Dtos.LoginRequest request);
    }
}
