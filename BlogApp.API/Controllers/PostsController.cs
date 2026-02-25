using BlogApp.Application.Features.Posts.Commands.CreatePost;
using BlogApp.Application.Features.Posts.Commands.DeletePost;
using BlogApp.Application.Features.Posts.Commands.UpdatePost;
using BlogApp.Application.Features.Posts.Queries.GetAllPosts;
using BlogApp.Application.Features.Posts.Queries.GetPostDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] GetAllPostsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreatePostCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetPostDetailQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpPut]
    public async Task<IActionResult> Update(UpdatePostCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok("Post updated.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeletePostCommand(id));
        return Ok("Post deleted.");
    }
}