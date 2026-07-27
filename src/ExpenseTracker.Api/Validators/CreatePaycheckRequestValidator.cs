using ExpenseTracker.Api.Dtos;
using FluentValidation;

namespace ExpenseTracker.Api.Validators
{
    public class CreatePaycheckRequestValidator : AbstractValidator<CreatePaycheckRequest>
    {
        public CreatePaycheckRequestValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(100).WithMessage("Description is required.");

            RuleFor(x => x.ReceivedDate)
                .NotEmpty().WithMessage("Received date is required.");

            // Validate each income in the list by reusing the income validator.
            RuleForEach(x => x.IncomesToCreate)
                .SetValidator(new CreateIncomeRequestValidator());
        }
    }
}
