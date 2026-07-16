namespace ExpenseTracker.Api.Dtos
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;

        public UserResponse User { get; set; } = null!;
    }
}
