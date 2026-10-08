using BlogApp.Application.Models.Requests;
using FluentValidation;

namespace BlogApp.Application.Validators;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.Title)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(100);

        RuleFor(request => request.Content).NotEmpty();
    }
}
