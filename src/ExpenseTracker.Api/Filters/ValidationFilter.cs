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
            context.Result = new BadRequestObjectResult(validationResult.Errors);
            return;
        }

        await next();

    }
}

