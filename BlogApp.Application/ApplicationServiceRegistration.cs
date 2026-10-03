using System.Reflection;
using BlogApp.Application.Mappings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BlogApp.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
        
        return services;
    }
    
}