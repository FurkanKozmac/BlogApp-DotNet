using BlogApp.Application.Interfaces.Repositories;
using MediatR;

namespace BlogApp.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, bool>
{
    private readonly IPostRepository _postRepository;

    public DeletePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id);

        if (post == null)
        {
            throw new Exception("Post not found.");
        }

        post.IsDeleted = true;

        await _postRepository.UpdateAsync(post);
        return true;
    }
}