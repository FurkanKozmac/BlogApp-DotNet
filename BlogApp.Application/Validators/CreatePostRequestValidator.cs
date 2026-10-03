using BlogApp.Application.Models.Requests;
using FluentValidation;

namespace BlogApp.Application.Validators;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(100);

        RuleFor(request => request.Content).NotEmpty();
    }
}
