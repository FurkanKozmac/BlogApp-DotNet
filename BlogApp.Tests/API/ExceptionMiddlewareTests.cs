using BlogApp.API.Middleware;
using BlogApp.Application.Common;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BlogApp.Tests.API;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task Invoke_WhenCredentialsAreInvalid_Returns401()
    {
        var context = await InvokeWithExceptionAsync(new InvalidCredentialsException());

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_WhenPostOwnershipIsDenied_Returns403()
    {
        var context = await InvokeWithExceptionAsync(new ForbiddenException("Not the owner."));

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    private static async Task<DefaultHttpContext> InvokeWithExceptionAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionMiddleware(_ => throw exception);

        await middleware.InvokeAsync(context);
        return context;
    }
}
