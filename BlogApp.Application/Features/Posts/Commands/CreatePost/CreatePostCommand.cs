using MediatR;

namespace BlogApp.Application.Features.Posts.Commands.CreatePost;

public record CreatePostCommand :  IRequest<int>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } =  string.Empty;
    public string Author { get; set; } = string.Empty;
}