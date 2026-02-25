using BlogApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, bool>
{
    private readonly UserManager<AppUser> _userManager;

    public RegisterCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.UserName
        };

        
        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            return true;
        }
        
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new Exception($"Registration Failed: {errors}");
        
    }
}