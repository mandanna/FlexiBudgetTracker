using ExpenseTracker.Api.Dtos;
using FluentValidation;

namespace ExpenseTracker.Api.Validators
{
    public class DashboardQueryRequestValidator : AbstractValidator<DashboardQueryRequest>
    {
        public DashboardQueryRequestValidator()
        {
            RuleFor(x => x.ToDate)
                .GreaterThanOrEqualTo(x=>x.FromDate)
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
                .WithMessage("ToDate must be greater than or equal to FromDate.");


        }
    }
}
