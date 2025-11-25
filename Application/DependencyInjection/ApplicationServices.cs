using Application.Interfaces.Repositories;
using Application.Sieve; // Nơi chứa CustomSieveProcessor
using FluentValidation;
using Microsoft.Extensions.Configuration; // THÊM CÁI NÀY
using Microsoft.Extensions.DependencyInjection;
using Sieve.Models; // THÊM CÁI NÀY ĐỂ FIX LỖI SieveOptions
using Sieve.Services;
using System.Reflection;

namespace Application.DependencyInjection;

public static class ApplicationServices
{
    // THÊM tham số IConfiguration configuration vào đây
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);

        // Giờ biến configuration đã tồn tại để dùng
        services.Configure<SieveOptions>(configuration.GetSection("Sieve"));

        // Đăng ký Custom Processor (OK vì class này nằm trong Application)
        services.AddScoped<ISieveProcessor, CustomSieveProcessor>();

        // --- XÓA DÒNG NÀY ĐI ---
        // services.AddScoped<IPaginator, SievePaginator>(); 
        // Lý do: Application không được biết SievePaginator là ai.

        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Behaviors.ValidationBehavior<,>));

        return services;
    }
}