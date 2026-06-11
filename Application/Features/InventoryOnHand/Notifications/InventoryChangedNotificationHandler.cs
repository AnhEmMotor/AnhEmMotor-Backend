using Application.Interfaces.Repositories.InventoryOnHand;
using MediatR;

namespace Application.Features.InventoryOnHand.Notifications;

public class InventoryChangedNotificationHandler(IInventoryOnHandUpdateRepository inventoryRepo) : INotificationHandler<InventoryChangedNotification>
{
    public async Task Handle(InventoryChangedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var combo in notification.Combos)
        {
            await inventoryRepo.RecalculateAsync(combo.VariantId, combo.ColorId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
