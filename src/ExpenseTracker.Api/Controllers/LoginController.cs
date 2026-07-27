using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Dtos.ResponseDtos.Category;
using ExpenseTracker.Api.Interface;
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

        public LoginController(IUserService userService)
        {
            _userService = userService;

        }
        [ServiceFilter(typeof(ValidationFilter<UserRegisterRequest>))]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            var user = await _userService.RegisterAsync(request);
            if (user == null)
            {

                return NotFound(new ApiResponse<List<CategoryResponse>>
                {
                    data = null,
                    message = "Email already exists.",
                    success = false
                });
            }
            return Ok(new ApiResponse<UserResponse>
            {
                success = true,
                message = "User registered successfully",
                data = user
            });

        }
        [ServiceFilter(typeof(ValidationFilter<Dtos.LoginRequest>))]
        [HttpPost("login")]
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
            return Ok(new ApiResponse<LoginResponse>
            {
                success = true,
                message = "User logged in successfully",
                data = user
            });
        }
    }
}
