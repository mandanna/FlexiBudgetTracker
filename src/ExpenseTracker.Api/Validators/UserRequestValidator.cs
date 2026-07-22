using ExpenseTracker.Api.Dtos;
using FluentValidation;

namespace ExpenseTracker.Api.Validators
{
    public class UserRequestValidator:AbstractValidator<UserRegisterRequest>
    {
        public UserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First Name is required");

                 RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last Name is required");

        }
    }
}
