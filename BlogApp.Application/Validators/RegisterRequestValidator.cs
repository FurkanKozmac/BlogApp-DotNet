using BlogApp.Application.Models.Requests;
using FluentValidation;

namespace BlogApp.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.UserName).NotEmpty().MaximumLength(256);
        RuleFor(request => request.Password).NotEmpty().MinimumLength(6);
    }
}
