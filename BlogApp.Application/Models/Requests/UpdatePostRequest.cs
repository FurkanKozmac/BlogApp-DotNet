namespace BlogApp.Application.Models.Requests;

public class UpdatePostRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
