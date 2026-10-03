using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BlogApp.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }
    
}