using BlogApp.Application.DTOs;
using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Application.Interfaces.Services;
using BlogApp.Application.Models.Requests;
using BlogApp.Domain.Entities;
using FluentValidation;

namespace BlogApp.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly IGenericRepository<Comment> _commentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateCommentRequest> _validator;

    public CommentService(
        IGenericRepository<Comment> commentRepository,
        ICurrentUserService currentUserService,
        IValidator<CreateCommentRequest> validator)
    {
        _commentRepository = commentRepository;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    public async Task<CommentDto> CreateAsync(CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var comment = new Comment
        {
            PostId = request.PostId,
            Text = request.Text,
            Author = _currentUserService.UserName ?? "Unknown"
        };
        await _commentRepository.AddAsync(comment);
        return new CommentDto
        {
            Id = comment.Id,
            Text = comment.Text,
            Author = comment.Author,
            CreatedDate = comment.CreatedDate
        };
    }
}
