using MediatR;

namespace BlogApp.Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<bool>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
