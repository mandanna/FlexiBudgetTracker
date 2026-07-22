using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Interface;
using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IPasswordHasherService _passwordHasher;
        private readonly ExpenseTrackerDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserService> _logger;

        public UserService(IPasswordHasherService passwordHasher,
            ExpenseTrackerDbContext context,
            IJwtService jwtService,ILogger<UserService> logger)
        {
            _passwordHasher = passwordHasher;
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(Dtos.LoginRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x=>x.Email==request.Email.ToLower());
                if (user == null)
                    return null;

                var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
                if (!isPasswordValid)
                    return null;
                var generatedToken = _jwtService.GenerateToken(user);

                if (string.IsNullOrEmpty(generatedToken))
                {
                    _logger.LogError("Token generation failed for user with email: {Email}", user.Email);
                    return null;
                }

                return new LoginResponse
                {
                    Token = generatedToken,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    }
                };
            }
            catch (Exception ex) {
                return null;
            }

        }

        public async Task<UserResponse?> RegisterAsync(UserRegisterRequest request)
        {
            var exists = await _context.Users.AnyAsync(x => x.Email == request.Email.ToLower());
            if (exists)
                throw new ConflictException("An account with this email already exists.");
            var user = new User()
            {
                Email = request.Email.Trim().ToLowerInvariant(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = _passwordHasher.HashPassword(request.Password)
            };
            await _context.AddAsync(user);
            try
            {

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException("An account with this email already exists.");
            }

            _logger.LogInformation("New user registered with email: {Email}", user.Email);
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

    }
}
