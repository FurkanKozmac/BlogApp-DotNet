using AutoMapper;
using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Application.Interfaces.Services;
using BlogApp.Domain.Entities;
using MediatR;

namespace BlogApp.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, int>
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;

    public CreatePostCommandHandler(IPostRepository postRepository, IMapper mapper, ICurrentUserService currentUserService, IFileService fileService)
    {
        _postRepository = postRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _fileService = fileService;
    }

    public async Task<int> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = _mapper.Map<Post>(request);
        if (request.Image != null)
        {
            post.ImageUrl = await _fileService.UploadFileAsync(request.Image);
        }
        post.AppUserId = _currentUserService.UserId;
        post.Author = _currentUserService.UserName ?? "Unknown";       
        await _postRepository.AddAsync(post);
        return post.Id;
    }
}
