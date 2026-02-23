using BlogApp.Domain.Common;

namespace BlogApp.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}