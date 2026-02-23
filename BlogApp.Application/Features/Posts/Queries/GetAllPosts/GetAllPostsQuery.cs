using BlogApp.Application.Common;
using BlogApp.Application.DTOs;
using MediatR;

namespace BlogApp.Application.Features.Posts.Queries.GetAllPosts;

public record GetAllPostsQuery : IRequest<PagedResult<PostDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}