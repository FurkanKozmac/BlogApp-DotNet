using FluentValidation;

namespace BlogApp.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(p => p.Title)
            .NotEmpty().WithMessage("Title can't be empty.")
            .MaximumLength(100).WithMessage("Title can contain maximum 100 characters.")
            .MinimumLength(5).WithMessage("Title must contain at least 5 characters.");

        RuleFor(p => p.Content)
            .NotEmpty().WithMessage("Content can't be empty.");

        // Will be deleted after auth is added.
        RuleFor(p => p.Author)
            .NotEmpty().WithMessage("Author can't be empty.");
    }
}