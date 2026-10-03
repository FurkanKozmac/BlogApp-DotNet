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

    public async Task<Post?> GetByIdWithCommentsAsync(int id)
    {
        var post = await _dbContext.Posts
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);
        
            return post;
    }
    
    public async Task<(IReadOnlyList<Post> Items, int TotalCount)> GetPagedWithCategoryAsync(int pageNumber, int pageSize)
    {
        var query = _dbContext.Posts
            .AsNoTracking()
            .Include(p => p.Category);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(post => post.CreatedDate)
            .ThenByDescending(post => post.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}