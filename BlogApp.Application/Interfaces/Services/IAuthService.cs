using BlogApp.Domain.Entities;

namespace BlogApp.Application.Interfaces.Services;

public interface IAuthService
{
    string GenerateToken(AppUser user);
}