using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Products.Notifications;

public class ProductChangedNotificationHandler(IProductIndexQueue queue) : INotificationHandler<ProductChangedNotification>
{
    public async Task Handle(ProductChangedNotification notification, CancellationToken cancellationToken)
    {
        await queue.EnqueueAsync(notification.ProductId, cancellationToken).ConfigureAwait(false);
    }
}
