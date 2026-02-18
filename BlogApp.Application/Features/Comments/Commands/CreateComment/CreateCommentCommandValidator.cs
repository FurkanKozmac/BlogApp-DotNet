using BlogApp.Application.Features.Posts.Commands.CreatePost;
using FluentValidation;

namespace BlogApp.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(p => p.Text)
            .NotEmpty().WithMessage("Comment can't be empty.");
    }
}