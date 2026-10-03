namespace BlogApp.Application.Models.Requests;

public class GetPostsRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
