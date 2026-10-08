using ExpenseTracker.Api.Dtos;
using FluentValidation;

namespace ExpenseTracker.Api.Validators
{
    public class CreateIncomeRequestValidator : AbstractValidator<CreateIncomeRequest>
    {
        public CreateIncomeRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.Source)
                 .NotEmpty().WithMessage("Source is required.")
    .MaximumLength(100).WithMessage("Source must be 100 characters or fewer.");

            RuleFor(x => x.ReceivedDate)
                .NotEmpty().WithMessage("Received date is required.");
        }
    }
}
