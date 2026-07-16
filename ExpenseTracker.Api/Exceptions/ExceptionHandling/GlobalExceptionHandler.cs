using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace ExpenseTracker.Api.Exceptions.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
           // _logger.LogError(exception,"Unhandled exception occurred. TraceId: {TraceId}",httpContext.TraceIdentifier);

            var (statusCode, message, logLevel) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, exception.Message,LogLevel.Information),
                ConflictException => (StatusCodes.Status409Conflict, exception.Message, LogLevel.Warning),
                BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, exception.Message, LogLevel.Warning),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Authentication is required to access this resource.", LogLevel.Warning),
                { InnerException: UnauthorizedAccessException } => (StatusCodes.Status401Unauthorized, "Authentication is required to access this resource.", LogLevel.Warning),


                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred (caught globally).\n" +exception.Message,LogLevel.Error)


            };
            if (logLevel == LogLevel.Error)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception for {RequestMethod} {RequestPath}. TraceId: {TraceId}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    httpContext.TraceIdentifier);
            }
            else
            {
                _logger.Log(
                    logLevel,
                    "Request rejected for {RequestMethod} {RequestPath} with status code {StatusCode}.Reason: {Reason}. TraceId: {TraceId}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    statusCode,
                    exception.Message,
                    httpContext.TraceIdentifier);
            }
            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                new
                {
                    Success = false,
                    Message = message,
                    TraceId = httpContext.TraceIdentifier
                },
                cancellationToken);

            return true;
        }
    }
}
