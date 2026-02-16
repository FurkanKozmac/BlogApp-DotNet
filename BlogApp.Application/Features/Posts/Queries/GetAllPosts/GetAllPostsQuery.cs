using BlogApp.Domain.Entities;
using MediatR;

namespace BlogApp.Application.Features.Posts.Queries.GetAllPosts;

public record GetAllPostsQuery : IRequest<List<Post>>;