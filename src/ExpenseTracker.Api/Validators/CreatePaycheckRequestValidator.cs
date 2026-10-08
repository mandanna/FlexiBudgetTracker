using ExpenseTracker.Api.Dtos;
using FluentValidation;

namespace ExpenseTracker.Api.Validators
{
    public class CreatePaycheckRequestValidator : AbstractValidator<CreatePaycheckRequest>
    {
        public CreatePaycheckRequestValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(100).WithMessage("Description must be 100 characters or fewer.");

            RuleFor(x => x.ReceivedDate)
                .NotEmpty().WithMessage("Received date is required.");

            //Validate each income in the list by reusing the income validator.
           RuleForEach(x => x.IncomesToCreate)
               .SetValidator(new CreateIncomeRequestValidator());
        }
    }
}
