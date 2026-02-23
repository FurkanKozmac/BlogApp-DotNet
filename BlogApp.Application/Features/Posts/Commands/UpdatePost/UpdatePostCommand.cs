using MediatR;

namespace BlogApp.Application.Features.Posts.Commands.UpdatePost;

public record UpdatePostCommand : IRequest<bool>
{
    public int Id { get; set; } 
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}