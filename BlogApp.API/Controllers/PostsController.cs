using BlogApp.Application.Features.Posts.Commands.CreatePost;
using BlogApp.Application.Features.Posts.Queries.GetAllPosts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPosts()
    {
        var query = new GetAllPostsQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePostCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(result);
    }
}