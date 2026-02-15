namespace BlogApp.Domain.Entities;

public sealed class Comment
{
    public string Text { get; set; } = String.Empty;
    public string Author { get; set; } = String.Empty;
    
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
}