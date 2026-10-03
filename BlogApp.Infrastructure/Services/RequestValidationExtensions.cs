using FluentValidation;

namespace BlogApp.Infrastructure.Services;

internal static class RequestValidationExtensions
{
    public static async Task ValidateAndThrowAsync<TRequest>(
        this IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
