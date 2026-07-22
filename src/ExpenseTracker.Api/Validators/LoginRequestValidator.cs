using FluentValidation;

namespace ExpenseTracker.Api.Validators
{
    public class LoginRequestValidator:AbstractValidator<Dtos.LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
               .NotEmpty().WithMessage("Email is required");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");

        }
    }
}
