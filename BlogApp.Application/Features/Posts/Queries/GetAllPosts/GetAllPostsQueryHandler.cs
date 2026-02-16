using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Domain.Entities;
using MediatR;

namespace BlogApp.Application.Features.Posts.Queries.GetAllPosts;

public class GetAllPostsQueryHandler : IRequestHandler<GetAllPostsQuery, List<Post>>
{
    private readonly IPostRepository _postRepository;

    public GetAllPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<Post>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetAllAsync();

        return posts.OrderByDescending(x => x.CreatedDate).ToList();
    }
}