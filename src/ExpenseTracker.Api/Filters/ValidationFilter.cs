using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ValidationFilter<T> : IAsyncActionFilter
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.ActionArguments.Values
       .OfType<T>()
       .FirstOrDefault();

        if (request == null)
        {
            await next();
            return;
        }
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var fieldErrors = validationResult.Errors
    .GroupBy(e => e.PropertyName.ToCamelCasePath())
    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            context.Result = new BadRequestObjectResult(new ApiResponse<Dictionary<string, string[]>>
            {
                success = false,
                message = "Validation failed.",
                data = fieldErrors
            });
            return;
        }

        await next();

    }
}

