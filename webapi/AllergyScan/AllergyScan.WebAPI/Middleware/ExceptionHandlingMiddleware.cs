using System.Text.Json;
using AllergyScan.BLL.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AllergyScan.WebAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // The client disconnected; there is nobody to send a response to.
            }
            catch (Exception exception)
            {
                await HandleAsync(context, exception);
            }
        }

        private async Task HandleAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                BadRequestException => (StatusCodes.Status400BadRequest, "Bad request"),
                NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Internal server error")
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Request {Method} {Path} failed with status {StatusCode}.",
                    context.Request.Method, context.Request.Path, statusCode);
            }
            else
            {
                _logger.LogWarning("Request {Method} {Path} failed with status {StatusCode}: {Message}",
                    context.Request.Method, context.Request.Path, statusCode, exception.Message);
            }

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started; the error response could not be written.");
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : exception.Message,
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem), context.RequestAborted);
        }
    }
}
