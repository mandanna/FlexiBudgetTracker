using ExpenseTracker.Api.Dtos;
using FluentValidation;

namespace ExpenseTracker.Api.Validators
{
    public class UpdateExpenseRequestValidator:AbstractValidator<UpdateExpenseRequest>
    {

        public UpdateExpenseRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("Description is required");

            RuleFor(x => x.ExpenseDate)
                .NotEmpty()
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("Expense date cannot be in the future.");


        }
    }
}
