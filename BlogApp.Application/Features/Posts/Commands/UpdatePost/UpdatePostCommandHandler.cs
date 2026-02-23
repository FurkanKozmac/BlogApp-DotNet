using BlogApp.Application.Interfaces.Repositories;
using MediatR;

namespace BlogApp.Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, bool>
{
    private readonly IPostRepository _postRepository;

    public UpdatePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<bool> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id);

        if (post == null || post.IsDeleted)
        {
            throw new Exception("Post not found.");
        }

        post.Title = request.Title;
        post.Content = request.Content;

        await _postRepository.UpdateAsync(post);

        return true;
    }
}