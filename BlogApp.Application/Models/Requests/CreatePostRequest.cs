using Microsoft.AspNetCore.Http;

namespace BlogApp.Application.Models.Requests;

public class CreatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public IFormFile? Image { get; set; }
}
