namespace BlogApp.Domain.Entities;

using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}