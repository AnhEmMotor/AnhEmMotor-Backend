using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants.Order;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services;

public class OrderCleanupService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CancelExpiredOrdersAsync(stoppingToken).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task CancelExpiredOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IOutputReadRepository>();
        var updateRepository = scope.ServiceProvider.GetRequiredService<IOutputUpdateRepository>();
        var expirationThreshold = DateTimeOffset.UtcNow.AddMinutes(-15);
        var expiredOrders = readRepository.GetQueryable()
            .Where(
                o => (o.StatusId == OrderStatus.Pending || o.StatusId == OrderStatus.WaitingDeposit) &&
                    !string.IsNullOrEmpty(o.PaymentMethod) &&
                    o.PaymentMethod != PaymentMethod.COD &&
                    (o.PaymentExpiredAt.HasValue
                        ? o.PaymentExpiredAt.Value < DateTimeOffset.UtcNow
                        : o.CreatedAt < expirationThreshold))
            .ToList();
        if (expiredOrders.Count > 0)
        {
            foreach (var order in expiredOrders)
            {
                order.StatusId = OrderStatus.Cancelled;
                updateRepository.Update(order);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
