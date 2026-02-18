namespace BlogApp.Application.DTOs;

public class PostDetailDto : PostDto
{
    public List<CommentDto> Comments { get; set; } = new();
}