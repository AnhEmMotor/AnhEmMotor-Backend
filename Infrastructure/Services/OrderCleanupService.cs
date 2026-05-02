using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants.Order;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class OrderCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderCleanupService> _logger;

    public OrderCleanupService(IServiceProvider serviceProvider, ILogger<OrderCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CancelExpiredOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during order cleanup");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CancelExpiredOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IOutputReadRepository>();
        var updateRepository = scope.ServiceProvider.GetRequiredService<IOutputUpdateRepository>();

        var expirationTime = DateTimeOffset.UtcNow.AddMinutes(-15);

        // Find orders created more than 15 minutes ago that are still pending/waiting deposit
        // Only cancel orders that are explicitly NOT COD (VNPay, PayOS)
        var expiredOrders = readRepository.GetQueryable()
            .Where(o => (o.StatusId == OrderStatus.Pending || o.StatusId == OrderStatus.WaitingDeposit) &&
                        !string.IsNullOrEmpty(o.PaymentMethod) && 
                        o.PaymentMethod != PaymentMethod.COD &&
                        o.CreatedAt < expirationTime)
            .ToList();

        if (expiredOrders.Any())
        {
            foreach (var order in expiredOrders)
            {
                order.StatusId = OrderStatus.Cancelled;
                updateRepository.Update(order);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
