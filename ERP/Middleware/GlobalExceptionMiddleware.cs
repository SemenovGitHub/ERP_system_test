using Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace ERP.Middleware;

/// <summary>
/// Global exception handling middleware that converts exceptions to appropriate HTTP responses
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    "VALIDATION_ERROR",
                    validationEx.Message,
                    validationEx.ValidationErrors
                )),
            
            BusinessException businessEx => (
                (HttpStatusCode)businessEx.StatusCode,
                new ErrorResponse(
                    businessEx.Code,
                    businessEx.Message,
                    null
                )),
            
            ArgumentException argumentEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    "INVALID_ARGUMENT",
                    argumentEx.Message,
                    null
                )),
            
            InvalidOperationException => (
                HttpStatusCode.Conflict,
                new ErrorResponse(
                    "INVALID_OPERATION",
                    "The requested operation is not valid in the current state",
                    null
                )),
            
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse(
                    "INTERNAL_ERROR",
                    "An internal server error occurred",
                    null
                ))
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Standard error response format
/// </summary>
public sealed record ErrorResponse(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationErrors
);