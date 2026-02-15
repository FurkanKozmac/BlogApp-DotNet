using BlogApp.Domain.Common;

namespace BlogApp.Domain.Entities;

public sealed class Post : BaseEntity
{
    public string Title { get; set; } =  String.Empty;
    public string Content { get; set; } = String.Empty;
    public string Author { get; set; } = String.Empty;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}