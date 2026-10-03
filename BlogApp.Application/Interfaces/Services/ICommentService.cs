using BlogApp.Application.DTOs;
using BlogApp.Application.Models.Requests;

namespace BlogApp.Application.Interfaces.Services;

public interface ICommentService
{
    Task<CommentDto> CreateAsync(CreateCommentRequest request, CancellationToken cancellationToken = default);
}
