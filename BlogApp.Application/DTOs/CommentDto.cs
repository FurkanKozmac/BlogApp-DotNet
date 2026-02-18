namespace BlogApp.Application.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string Text { get; set; } = String.Empty;
    public string Author { get; set; } = String.Empty;
    public DateTime CreatedDate { get; set; }
}