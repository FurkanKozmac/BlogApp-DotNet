using BlogApp.Application.Common;
using BlogApp.Application.DTOs;
using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Application.Interfaces.Services;
using BlogApp.Application.Models.Requests;
using BlogApp.Domain.Entities;
using FluentValidation;

namespace BlogApp.Infrastructure.Services;

public class BlogPostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileService _fileService;
    private readonly IValidator<GetPostsRequest> _getPostsValidator;
    private readonly IValidator<CreatePostRequest> _createPostValidator;
    private readonly IValidator<UpdatePostRequest> _updatePostValidator;

    public BlogPostService(
        IPostRepository postRepository,
        ICurrentUserService currentUserService,
        IFileService fileService,
        IValidator<GetPostsRequest> getPostsValidator,
        IValidator<CreatePostRequest> createPostValidator,
        IValidator<UpdatePostRequest> updatePostValidator)
    {
        _postRepository = postRepository;
        _currentUserService = currentUserService;
        _fileService = fileService;
        _getPostsValidator = getPostsValidator;
        _createPostValidator = createPostValidator;
        _updatePostValidator = updatePostValidator;
    }

    public async Task<PagedResult<PostDto>> GetAllAsync(GetPostsRequest request, CancellationToken cancellationToken = default)
    {
        await _getPostsValidator.ValidateAndThrowAsync(request, cancellationToken);
        var (posts, totalCount) = await _postRepository.GetPagedWithCategoryAsync(request.PageNumber, request.PageSize);
        var items = posts.Select(ToPostDto).ToList();

        return new PagedResult<PostDto>(items, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<PostDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdWithCommentsAsync(id);
        if (post == null)
        {
            throw new Exception("Post no exists.");
        }

        return ToPostDetailDto(post);
    }

    public async Task<int> CreateAsync(CreatePostRequest request, CancellationToken cancellationToken = default)
    {
        await _createPostValidator.ValidateAndThrowAsync(request, cancellationToken);
        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            CategoryId = request.CategoryId
        };
        if (request.Image != null)
        {
            post.ImageUrl = await _fileService.UploadFileAsync(request.Image);
        }

        post.AppUserId = _currentUserService.UserId;
        post.Author = _currentUserService.UserName ?? "Unknown";
        await _postRepository.AddAsync(post);
        return post.Id;
    }

    private static PostDto ToPostDto(Post post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Content = post.Content,
        Author = post.Author,
        CreatedDate = post.CreatedDate,
        CategoryName = post.Category?.Name ?? string.Empty,
        ImageUrl = post.ImageUrl
    };

    private static PostDetailDto ToPostDetailDto(Post post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Content = post.Content,
        Author = post.Author,
        CreatedDate = post.CreatedDate,
        CategoryName = post.Category?.Name ?? string.Empty,
        ImageUrl = post.ImageUrl,
        Comments = post.Comments.Select(comment => new CommentDto
        {
            Id = comment.Id,
            Text = comment.Text,
            Author = comment.Author,
            CreatedDate = comment.CreatedDate
        }).ToList()
    };

    public async Task UpdateAsync(UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        await _updatePostValidator.ValidateAndThrowAsync(request, cancellationToken);
        var post = await _postRepository.GetByIdAsync(request.Id);
        if (post == null || post.IsDeleted)
        {
            throw new Exception("Post not found.");
        }

        if (post.AppUserId != _currentUserService.UserId && !_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("It is not your post.");
        }

        post.Title = request.Title;
        post.Content = request.Content;
        await _postRepository.UpdateAsync(post);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            throw new Exception("Post not found.");
        }

        if (post.AppUserId != _currentUserService.UserId && !_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("It is not your post.");
        }

        post.IsDeleted = true;
        await _postRepository.UpdateAsync(post);
    }
}
