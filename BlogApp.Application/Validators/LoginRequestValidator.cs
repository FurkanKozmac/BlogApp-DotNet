using BlogApp.Application.Models.Requests;
using FluentValidation;

namespace BlogApp.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.UserName).NotEmpty();
        RuleFor(request => request.Password).NotEmpty();
    }
}
