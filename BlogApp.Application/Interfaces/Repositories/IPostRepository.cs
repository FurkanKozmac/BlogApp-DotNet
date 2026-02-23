using BlogApp.Domain.Entities;

namespace BlogApp.Application.Interfaces.Repositories;

public interface IPostRepository : IGenericRepository<Post>
{
    Task<Post> GetByIdWithCommentsAsync(int id);
    Task<IReadOnlyList<Post>> GetAllWithCategoryAsync();
}