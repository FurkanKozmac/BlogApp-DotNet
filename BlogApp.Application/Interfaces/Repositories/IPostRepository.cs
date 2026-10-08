using BlogApp.Domain.Entities;

namespace BlogApp.Application.Interfaces.Repositories;

public interface IPostRepository : IGenericRepository<Post>
{
    Task<Post?> GetByIdWithCommentsAsync(int id);
    Task<(IReadOnlyList<Post> Items, int TotalCount)> GetPagedWithCategoryAsync(int pageNumber, int pageSize);
}