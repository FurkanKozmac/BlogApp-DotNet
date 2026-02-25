using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Application.Interfaces.Services;
using MediatR;

namespace BlogApp.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, bool>
{
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUserService _currentUserService;


    public DeletePostCommandHandler(IPostRepository postRepository, ICurrentUserService currentUserService)
    {
        _postRepository = postRepository;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id);

        if (post == null)
        {
            throw new Exception("Post not found.");
        }
        
        var currentUserId = _currentUserService.UserId;
        
        if (post.AppUserId != currentUserId && !_currentUserService.IsAdmin) throw new Exception("It is not your post!");

        post.IsDeleted = true;
        await _postRepository.UpdateAsync(post);
        return true;
    }
}