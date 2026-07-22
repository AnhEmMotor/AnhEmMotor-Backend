using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants.Order;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services;

public class OrderCleanupService(IServiceProvider serviceProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await CancelExpiredOrdersAsync(stoppingToken).ConfigureAwait(false);
				await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
		}
	}

	private async Task CancelExpiredOrdersAsync(CancellationToken cancellationToken)
	{
		using var scope = serviceProvider.CreateScope();
		var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
		var readRepository = scope.ServiceProvider.GetRequiredService<IOutputReadRepository>();
		var updateRepository = scope.ServiceProvider.GetRequiredService<IOutputUpdateRepository>();
		var dbContext = scope.ServiceProvider.GetRequiredService<Infrastructure.DBContexts.ApplicationDBContext>();

		var expirationThreshold = DateTimeOffset.UtcNow.AddMinutes(-15);
		var expiredOrders = await readRepository.GetExpiredOrdersAsync(expirationThreshold, cancellationToken)
			.ConfigureAwait(false);
		if (expiredOrders.Count > 0)
		{
			foreach (var order in expiredOrders)
			{
				var oldStatusId = order.StatusId;
				order.Buyer = null;
				order.FinishedByUser = null;
				order.OutputInfos = null;
				order.OutputStatus = null;
				order.StatusId = OrderStatus.Cancelled;
				order.LastStatusChangedAt = DateTimeOffset.UtcNow;
				updateRepository.Update(order);

				dbContext.OrderStatusHistories.Add(new Domain.Entities.OrderStatusHistory
				{
					OutputId = order.Id,
					FromStatus = oldStatusId,
					ToStatus = OrderStatus.Cancelled,
					Note = "Hệ thống tự động hủy do quá hạn thanh toán 24h.",
					ChangedAt = DateTimeOffset.UtcNow,
					ChangedBy = null
				});
			}
			await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}
