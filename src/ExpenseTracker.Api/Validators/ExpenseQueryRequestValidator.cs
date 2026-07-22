using ExpenseTracker.Api.Dtos;
using FluentValidation;

namespace ExpenseTracker.Api.Validators
{
    public class ExpenseQueryRequestValidator : AbstractValidator<ExpenseQueryRequest>
    {

        private static readonly string[] AllowedSortFields =
            ["Description", "Amount", "ExpenseDate", "CategoryName"];

        public ExpenseQueryRequestValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

            RuleFor(x => x.SortBy)
                .Must(sortBy => sortBy is null || AllowedSortFields.Contains(sortBy.Trim(), StringComparer.OrdinalIgnoreCase))
                .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");

            RuleFor(x => x.ToDate)
                .GreaterThanOrEqualTo(x => x.FromDate)
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
                .WithMessage("ToDate must be on or after FromDate.");
        }

    }
}
