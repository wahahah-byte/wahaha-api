using System.Net;
using System.Text.Json;
using Azure;

namespace wahaha.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, message) = exception switch
        {
            RequestFailedException azureEx => HandleAzureException(azureEx, traceId),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "You do not have permission to perform this action."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message),
            InvalidOperationException invEx => (HttpStatusCode.BadRequest, invEx.Message),
            _ => HandleUnexpectedException(exception, traceId)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            traceId
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private (HttpStatusCode, string) HandleAzureException(RequestFailedException ex, string traceId)
    {
        _logger.LogError(ex, "Azure service error. TraceId: {TraceId}", traceId);

        return ex.Status switch
        {
            404 => (HttpStatusCode.NotFound, "The requested Azure resource was not found."),
            403 => (HttpStatusCode.Forbidden, "Access to the Azure resource was denied."),
            409 => (HttpStatusCode.Conflict, "A conflict occurred with the Azure resource."),
            _ => (HttpStatusCode.ServiceUnavailable, "An Azure service error occurred. Please try again later.")
        };
    }

    private (HttpStatusCode, string) HandleUnexpectedException(Exception ex, string traceId)
    {
        _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);
        return (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.");
    }
}