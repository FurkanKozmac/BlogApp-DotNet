using BlogApp.Application.Interfaces.Services;
using BlogApp.Application.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] GetPostsRequest request, CancellationToken cancellationToken)
    {
        var result = await _postService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreatePostRequest request, CancellationToken cancellationToken)
    {
        var id = await _postService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _postService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }
    
    [HttpPut]
    public async Task<IActionResult> Update(UpdatePostRequest request, CancellationToken cancellationToken)
    {
        await _postService.UpdateAsync(request, cancellationToken);
        return Ok("Post updated.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _postService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}