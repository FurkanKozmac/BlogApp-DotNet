using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Domain.Entities;
using MediatR;

namespace BlogApp.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, int>
{
    private readonly IPostRepository _postRepository;

    public CreatePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<int> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            Author = request.Author
        };

        await _postRepository.AddAsync(post);
        return post.Id;
    }
}
