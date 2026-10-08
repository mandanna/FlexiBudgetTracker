using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Dtos.ResponseDtos.Category;
using ExpenseTracker.Api.Interface;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWebHostEnvironment _env;

        public LoginController(IUserService userService, IWebHostEnvironment env, ICurrentUserService currentUserService)
        {
            _userService = userService;
            _env = env;
            _currentUserService = currentUserService;

        }
        [ServiceFilter(typeof(ValidationFilter<UserRegisterRequest>))]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            var user = await _userService.RegisterAsync(request);
            return Ok(new ApiResponse<UserResponse>
            {
                success = true,
                message = "User registered successfully",
                data = user
            });

        }
        [ServiceFilter(typeof(ValidationFilter<Dtos.LoginRequest>))]
        [HttpPost]
        public async Task<IActionResult> Login(Dtos.LoginRequest request)
        {
            var user = await _userService.LoginAsync(request);
           
            if (user == null)
            {
                return NotFound(new ApiResponse<LoginResponse>
                {
                    data = null,
                    message = "Invalid email or password.",
                    success = false
                });
            }

            Response.Cookies.Append("token", user?.Token ?? "", new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
            user.Token = string.Empty;    
            return Ok(new ApiResponse<LoginResponse>
            {
                success = true,
                message = "User logged in successfully",
                data = user
            });
        }
        [Authorize]
        [HttpGet("Me")]
        public async Task<IActionResult> Me()
        {
            var user = await _userService.GetByIdAsync(_currentUserService.UserId);
            if (user == null)
            {
                return Unauthorized(new ApiResponse<UserResponse>
                {
                    data = null,
                    message = "Not authenticated.",
                    success = false
                });
            }

            return Ok(new ApiResponse<UserResponse>
            {
                success = true,
                message = "Current user",
                data = user
            });
        }
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token", new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            return Ok(new ApiResponse<string>
            {
                success = true,
                message = "Logged out successfully",
                data = null
            });
        }
    }
}
