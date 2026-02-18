using BlogApp.Application.DTOs;
using MediatR;

namespace BlogApp.Application.Features.Posts.Queries.GetPostDetail;

public record GetPostDetailQuery(int PostId) : IRequest<PostDetailDto>;