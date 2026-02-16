using BlogApp.Domain.Common;
using BlogApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Infrastructure.Persistence;

public class BlogDbContext: DbContext
{
    
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }
    
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedDate = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
    
}