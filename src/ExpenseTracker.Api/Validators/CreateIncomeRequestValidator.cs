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
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Source is required.");

            RuleFor(x => x.ReceivedDate)
                .NotEmpty().WithMessage("Received date is required.");
        }
    }
}
