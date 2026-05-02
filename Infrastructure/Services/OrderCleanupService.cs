using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants.Order;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class OrderCleanupService(IServiceProvider serviceProvider, ILogger<OrderCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CancelExpiredOrdersAsync(stoppingToken).ConfigureAwait(false);
            } catch(Exception ex)
            {
                logger.LogError(ex, "Error occurred during order cleanup");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task CancelExpiredOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IOutputReadRepository>();
        var updateRepository = scope.ServiceProvider.GetRequiredService<IOutputUpdateRepository>();

        var expirationTime = DateTimeOffset.UtcNow.AddMinutes(-15);

        var expiredOrders = readRepository.GetQueryable()
            .Where(
                o => (o.StatusId == OrderStatus.Pending || o.StatusId == OrderStatus.WaitingDeposit) &&
                    !string.IsNullOrEmpty(o.PaymentMethod) &&
                    o.PaymentMethod != PaymentMethod.COD &&
                    o.CreatedAt < expirationTime)
            .ToList();

        if(expiredOrders.Count == 0)
        {
            foreach(var order in expiredOrders)
            {
                order.StatusId = OrderStatus.Cancelled;
                updateRepository.Update(order);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
