
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace BlogApp.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(httpContext, e);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var statusCode = (int)HttpStatusCode.InternalServerError;
        object errors = null;
        
        if (exception is ValidationException validationException)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            errors = validationException.Errors.Select(x => x.ErrorMessage);
        }

        var result = JsonSerializer.Serialize(new { 
            StatusCode = statusCode, 
            Message = errors ?? exception.Message 
        });
        
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(result);
    }
}