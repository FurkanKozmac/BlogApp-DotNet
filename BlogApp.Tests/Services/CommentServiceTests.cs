using BlogApp.Application.DTOs;
using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Application.Interfaces.Services;
using BlogApp.Application.Models.Requests;
using BlogApp.Application.Validators;
using BlogApp.Domain.Entities;
using BlogApp.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BlogApp.Tests.Services;

public class CommentServiceTests
{
    [Fact]
    public async Task CreateComment_WhenRequestHasNoAuthor_UsesAuthenticatedUserName()
    {
        var repository = new FakeCommentRepository();
        var service = new CommentService(
            repository,
            new FakeCurrentUserService("user-1", "jane"),
            new CreateCommentRequestValidator());

        var comment = await service.CreateAsync(new CreateCommentRequest { PostId = 3, Text = "Nice post" });

        Assert.Equal("jane", repository.SavedComment?.Author);
        Assert.Equal("jane", comment.Author);
    }

    private sealed class FakeCommentRepository : IGenericRepository<Comment>
    {
        public Comment? SavedComment { get; private set; }

        public Task<IReadOnlyList<Comment>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<Comment>>(Array.Empty<Comment>());

        public Task<Comment?> GetByIdAsync(int id) => Task.FromResult<Comment?>(null);

        public Task<Comment> AddAsync(Comment entity)
        {
            entity.Id = 12;
            SavedComment = entity;
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(Comment entity) => Task.CompletedTask;

        public Task DeleteAsync(Comment entity) => Task.CompletedTask;
    }

    private sealed class FakeCurrentUserService(string userId, string userName) : ICurrentUserService
    {
        public string? UserId { get; } = userId;
        public string? UserName { get; } = userName;
        public bool IsAdmin => false;
    }
}
