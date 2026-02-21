using MediatR;

namespace BlogApp.Application.Features.Auth.Commands.Login;

public record LoginCommand : IRequest<string> // Geriye Token (string) dönecek
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}