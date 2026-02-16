using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Domain.Entities;
using BlogApp.Infrastructure.Persistence;

namespace BlogApp.Infrastructure.Repositories;

public class PostRepository : GenericRepository<Post>, IPostRepository
{
    public PostRepository(BlogDbContext dbContext) : base(dbContext)
    {
       
    }
    
   
}