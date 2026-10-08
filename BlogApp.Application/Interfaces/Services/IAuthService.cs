using BlogApp.Application.Models.Requests;

namespace BlogApp.Application.Interfaces.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<string> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}