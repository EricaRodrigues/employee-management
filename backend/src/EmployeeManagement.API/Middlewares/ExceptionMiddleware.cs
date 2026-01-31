using EmployeeManagement.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace EmployeeManagement.API.Middlewares;

// Handles global exceptions
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            // Business rule error (expected)
            logger.LogWarning(ex, "Business rule violation");

            await HandleExceptionAsync(
                context,
                HttpStatusCode.BadRequest,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            // Unexpected error
            logger.LogError(ex, "Unhandled exception");

            await HandleExceptionAsync(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred."
            );
        }
    }
    
    
    private static async Task HandleExceptionAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message
    )
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response)
        );
    }
}