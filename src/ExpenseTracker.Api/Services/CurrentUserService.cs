using ExpenseTracker.Api.Interface;
using System.Security.Claims;

namespace ExpenseTracker.Api.Services
{
    public class CurrentUserService:ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId {
            get {
                var claim = _httpContextAccessor.HttpContext?
                   .User
                   .FindFirst(ClaimTypes.NameIdentifier)?
                   .Value;

                if (!int.TryParse(claim, out var userId))
                {
                    throw new UnauthorizedAccessException(
                        "No authenticated user.");
                }

                return userId;
            }
        }
    }
}


