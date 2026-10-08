using BlogApp.Application.Common;
using BlogApp.Application.DTOs;
using BlogApp.Application.Models.Requests;

namespace BlogApp.Application.Interfaces.Services;

public interface IPostService
{
    Task<PagedResult<PostDto>> GetAllAsync(GetPostsRequest request, CancellationToken cancellationToken = default);
    Task<PostDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreatePostRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdatePostRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
