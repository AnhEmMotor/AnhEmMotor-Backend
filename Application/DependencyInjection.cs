using Application.Sieve;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;
using System.Reflection;

namespace Application; // Rút gọn namespace, không cần .DependencyInjection

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 1. MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Đăng ký Pipeline Behaviors (Validation, Logging, etc.)
            cfg.AddOpenBehavior(typeof(Behaviors.ValidationBehavior<,>));
        });

        // 2. Validators
        services.AddValidatorsFromAssembly(assembly);

        // 3. Mapster (Chuyển từ Infra/WebAPI về đây)
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        // 4. Sieve Processor
        services.AddScoped<ISieveProcessor, CustomSieveProcessor>();

        return services;
    }
}