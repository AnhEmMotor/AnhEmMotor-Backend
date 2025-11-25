using Application.Sieve;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Models;
using Sieve.Services;
using System.Reflection;

namespace Application.DependencyInjection;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(
            cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });

        services.AddValidatorsFromAssembly(assembly);

        services.Configure<SieveOptions>(configuration.GetSection("Sieve"));

        services.AddScoped<ISieveProcessor, CustomSieveProcessor>();

        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Behaviors.ValidationBehavior<,>));

        return services;
    }
}