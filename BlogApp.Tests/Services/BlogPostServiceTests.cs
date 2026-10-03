using BlogApp.Application.Common;
using BlogApp.Application.DTOs;
using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Application.Interfaces.Services;
using BlogApp.Application.Models.Requests;
using BlogApp.Application.Validators;
using BlogApp.Domain.Entities;
using BlogApp.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BlogApp.Tests.Services;

public class BlogPostServiceTests
{
    [Fact]
    public async Task UpdatePost_WhenUserIsNotOwner_ThrowsForbidden()
    {
        var repository = new FakePostRepository(
            new Post { Id = 1, AppUserId = "owner", Title = "Original", Content = "Content" });
        var service = CreateService(repository, new FakeCurrentUserService("current-user", isAdmin: false));

        await Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateAsync(new UpdatePostRequest
        {
            Id = 1,
            Title = "Changed title",
            Content = "Changed content"
        }));

        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task CreatePost_WhenRequestIsInvalid_ThrowsValidationException()
    {
        var repository = new FakePostRepository();
        var service = CreateService(repository, new FakeCurrentUserService("user", isAdmin: false));

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new CreatePostRequest
        {
            Title = "tiny",
            Content = string.Empty
        }));

        Assert.Equal(0, repository.AddCount);
    }

    [Fact]
    public async Task UpdatePost_WhenUserIsAdmin_UpdatesAnotherUsersPost()
    {
        var repository = new FakePostRepository(
            new Post { Id = 1, AppUserId = "owner", Title = "Original", Content = "Content" });
        var service = CreateService(repository, new FakeCurrentUserService("admin", isAdmin: true));

        await service.UpdateAsync(new UpdatePostRequest
        {
            Id = 1,
            Title = "Updated title",
            Content = "Updated content"
        });

        Assert.Equal(1, repository.UpdateCount);
        Assert.Equal("Updated title", repository.Posts.Single().Title);
        Assert.Equal("Updated content", repository.Posts.Single().Content);
    }

    [Fact]
    public async Task CreateAsync_MapsValidRequestAndSavesPost()
    {
        var repository = new FakePostRepository();
        var service = CreateService(repository, new FakeCurrentUserService("current-user", isAdmin: false));

        var id = await service.CreateAsync(new CreatePostRequest
        {
            Title = "Valid title",
            Content = "Valid content",
            CategoryId = 7
        });

        Assert.Equal(1, id);
        Assert.Equal(1, repository.AddCount);
        Assert.Equal("current-user", repository.SavedPost?.AppUserId);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1_000_001, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetPosts_WhenPageOrPageSizeIsInvalid_ThrowsValidationException(int pageNumber, int pageSize)
    {
        var repository = new FakePostRepository();
        var service = CreateService(repository, new FakeCurrentUserService("user", isAdmin: false));

        await Assert.ThrowsAsync<ValidationException>(() => service.GetAllAsync(new GetPostsRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        }));

        Assert.Equal(0, repository.PagedQueryCount);
    }

    [Fact]
    public async Task GetPosts_WhenPageSizeIsAtMaximum_UsesRequestedPage()
    {
        var repository = new FakePostRepository();
        var service = CreateService(repository, new FakeCurrentUserService("user", isAdmin: false));

        var result = await service.GetAllAsync(new GetPostsRequest
        {
            PageNumber = 1,
            PageSize = GetPostsRequestValidator.MaximumPageSize
        });

        Assert.Equal(1, repository.LastPageNumber);
        Assert.Equal(GetPostsRequestValidator.MaximumPageSize, repository.LastPageSize);
        Assert.Equal(GetPostsRequestValidator.MaximumPageSize, result.PageSize);
    }

    private static BlogPostService CreateService(FakePostRepository repository, ICurrentUserService currentUser)
    {
        return new BlogPostService(
            repository,
            currentUser,
            new FakeFileService(),
            new GetPostsRequestValidator(),
            new CreatePostRequestValidator(),
            new UpdatePostRequestValidator());
    }

    private sealed class FakePostRepository(params Post[] posts) : IPostRepository
    {
        private readonly List<Post> _posts = [.. posts];

        public int AddCount { get; private set; }
        public int UpdateCount { get; private set; }
        public int PagedQueryCount { get; private set; }
        public int LastPageNumber { get; private set; }
        public int LastPageSize { get; private set; }
        public Post? SavedPost { get; private set; }
        public IReadOnlyList<Post> Posts => _posts;

        public Task<IReadOnlyList<Post>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<Post>>(_posts);

        public Task<Post?> GetByIdAsync(int id) =>
            Task.FromResult(_posts.SingleOrDefault(post => post.Id == id));

        public Task<Post> AddAsync(Post entity)
        {
            AddCount++;
            entity.Id = _posts.Count + 1;
            _posts.Add(entity);
            SavedPost = entity;
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(Post entity)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Post entity)
        {
            _posts.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<Post?> GetByIdWithCommentsAsync(int id) =>
            Task.FromResult(_posts.SingleOrDefault(post => post.Id == id));

        public Task<(IReadOnlyList<Post> Items, int TotalCount)> GetPagedWithCategoryAsync(int pageNumber, int pageSize)
        {
            PagedQueryCount++;
            LastPageNumber = pageNumber;
            LastPageSize = pageSize;
            return Task.FromResult<(IReadOnlyList<Post>, int)>((_posts, _posts.Count));
        }
    }

    private sealed class FakeCurrentUserService(string? userId, bool isAdmin) : ICurrentUserService
    {
        public string? UserId { get; } = userId;
        public string? UserName => "current-user";
        public bool IsAdmin { get; } = isAdmin;
    }

    private sealed class FakeFileService : IFileService
    {
        public Task<string> UploadFileAsync(IFormFile file) => Task.FromResult("/uploads/test.png");
    }
}
