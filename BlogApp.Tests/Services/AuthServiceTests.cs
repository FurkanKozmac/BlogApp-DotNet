using BlogApp.Application.Common;
using BlogApp.Application.Models.Requests;
using BlogApp.Application.Validators;
using BlogApp.Domain.Entities;
using BlogApp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BlogApp.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task Login_WhenUserNameDoesNotExist_ThrowsInvalidCredentials()
    {
        var service = CreateService(new FakeUserManager(user: null, passwordValid: false));

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.LoginAsync(new LoginRequest
        {
            UserName = "unknown",
            Password = "password123"
        }));
    }

    [Fact]
    public async Task Login_WhenPasswordIsIncorrect_ThrowsInvalidCredentials()
    {
        var service = CreateService(new FakeUserManager(
            new AppUser { Id = "user-1", UserName = "jane", Email = "jane@example.com", FullName = "Jane Doe" },
            passwordValid: false));

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.LoginAsync(new LoginRequest
        {
            UserName = "jane",
            Password = "wrong-password"
        }));
    }

    private static AuthService CreateService(UserManager<AppUser> userManager)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        return new AuthService(
            configuration,
            userManager,
            new RegisterRequestValidator(),
            new LoginRequestValidator());
    }

    private sealed class FakeUserManager(AppUser? user, bool passwordValid)
        : UserManager<AppUser>(
            new FakeUserStore(),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            new PasswordHasher<AppUser>(),
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<UserManager<AppUser>>.Instance)
    {
        public override Task<AppUser?> FindByNameAsync(string userName) => Task.FromResult(user);

        public override Task<bool> CheckPasswordAsync(AppUser user, string password) =>
            Task.FromResult(passwordValid);
    }

    private sealed class FakeUserStore : IUserStore<AppUser>
    {
        public Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResult.Success);

        public Task<AppUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
            Task.FromResult<AppUser?>(null);

        public Task<AppUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
            Task.FromResult<AppUser?>(null);

        public Task<string> GetUserIdAsync(AppUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Id);

        public Task<string?> GetUserNameAsync(AppUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.UserName);

        public Task SetUserNameAsync(AppUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.NormalizedUserName);

        public Task SetNormalizedUserNameAsync(AppUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
