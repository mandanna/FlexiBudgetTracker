using ExpenseTracker.Api.Interface;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Security.Claims;
using Serilog.Context;

namespace ExpenseTracker.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";


            using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("RequestMethod", context.Request.Method))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            {
                _logger.LogInformation("Incoming {Method} {Path}", context.Request.Method, context.Request.Path);
                try
                {
                    await _next(context);
                }
                finally
                {
                    stopwatch.Stop();
                    var statusCode = context.Response.StatusCode;
                    if (statusCode >= 500)
                    {
                        _logger.LogError(
                            "Request completed with status {StatusCode} in {ElapsedMilliseconds} ms",
                            statusCode,
                            stopwatch.ElapsedMilliseconds);
                    }
                    else if (statusCode >= 400)
                    {
                        _logger.LogWarning(
                            "Request completed with status {StatusCode} in {ElapsedMilliseconds} ms",
                            statusCode,
                            stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Request completed with status {StatusCode} in {ElapsedMilliseconds} ms",
                            statusCode,
                            stopwatch.ElapsedMilliseconds);
                    }
                }

            }
        }

    }
}