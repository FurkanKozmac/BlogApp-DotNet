using BlogApp.Application.Interfaces.Services;
using BlogApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler: IRequestHandler<LoginCommand, string>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IAuthService _authService;

    public LoginCommandHandler(UserManager<AppUser> userManager, IAuthService authService)
    {
        _userManager = userManager;
        _authService = authService;
    }

    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {

        var user = await _userManager.FindByNameAsync(request.UserName);
        
        if (user == null) throw new Exception("User couldn't found.");
        
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        
        if (!isPasswordValid) throw new Exception("Wrong password.");
        
        return _authService.GenerateToken(user);
    }
}