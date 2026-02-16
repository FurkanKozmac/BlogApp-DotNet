using BlogApp.Domain.Common;

namespace BlogApp.Domain.Entities;

public sealed class Comment : BaseEntity
{
    public string Text { get; set; } = String.Empty;
    public string Author { get; set; } = String.Empty;
    
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
}