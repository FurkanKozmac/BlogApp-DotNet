using BlogApp.Application.DTOs;
using MediatR;

namespace BlogApp.Application.Features.Posts.Queries.GetAllPosts;

public record GetAllPostsQuery : IRequest<List<PostDto>>;