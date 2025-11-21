using Application.Interfaces.Services;
using Application.Services.File;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.DependencyInjection;

/// <summary>
/// Cấu hình Dependency Injection cho Application layer
/// </summary>
public static class ApplicationServices
{
    /// <summary>
    /// Đăng ký các dịch vụ từ Application layer: MediatR handlers, FluentValidation validators, và Behaviors
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection đã được cấu hình</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Behaviors.ValidationBehavior<,>));

        services.AddScoped<IFileSelectService, FileSelectService>();

        return services;
    }
}
