using BlogApp.Application.DTOs;
using MediatR;

namespace BlogApp.Application.Features.Comments.Commands.CreateComment;

public record CreateCommentCommand : IRequest<CommentDto>
{
    public int PostId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}
