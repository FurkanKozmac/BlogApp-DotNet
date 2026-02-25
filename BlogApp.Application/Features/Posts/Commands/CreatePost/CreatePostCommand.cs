using BlogApp.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BlogApp.Application.Features.Posts.Commands.CreatePost;

public record CreatePostCommand :  IRequest<int>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } =  string.Empty;
    public int CategoryId { get; set; }
    public IFormFile? Image { get; set; }
}