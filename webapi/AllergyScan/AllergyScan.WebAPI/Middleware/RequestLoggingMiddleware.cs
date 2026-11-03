using System.Diagnostics;

namespace AllergyScan.WebAPI.Middleware
{
    /// <summary>Writes one console log line for every HTTP request, including its final status code and duration.</summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                // The exception middleware normally handles this; log it here as a safety net
                // so an exception is never silently lost.
                _logger.LogError(
                    exception,
                    "Unhandled exception while processing {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path);
                throw;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "{Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMilliseconds} ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
