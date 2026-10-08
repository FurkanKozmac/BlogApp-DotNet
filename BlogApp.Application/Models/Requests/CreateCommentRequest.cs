namespace BlogApp.Application.Models.Requests;

public class CreateCommentRequest
{
    public int PostId { get; set; }
    public string Text { get; set; } = string.Empty;
}
