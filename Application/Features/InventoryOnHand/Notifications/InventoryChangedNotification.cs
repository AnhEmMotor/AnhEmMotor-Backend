using System.Collections.Generic;
using MediatR;

namespace Application.Features.InventoryOnHand.Notifications;

public record InventoryChangedNotification(HashSet<(int VariantId, int? ColorId)> Combos) : INotification;
