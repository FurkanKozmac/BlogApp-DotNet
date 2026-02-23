using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Domain.Entities;
using BlogApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Infrastructure.Repositories;

public class PostRepository : GenericRepository<Post>, IPostRepository
{
    public PostRepository(BlogDbContext dbContext) : base(dbContext)
    {
       
    }

    public async Task<Post> GetByIdWithCommentsAsync(int id)
    {
        var post = await _dbContext.Posts
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);
        
            return post;
    }
    
    public async Task<IReadOnlyList<Post>> GetAllWithCategoryAsync()
    {
        return await _dbContext.Posts
            .Include(p => p.Category) // Kategoriyi JOIN yap
            .ToListAsync();
    }
}