using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using MediatR;

namespace Application.Features.Logistics.Commands.ProcessFulfillment;

public class ToggleItemPickCommandHandler(
    IParcelDeliveryOrderReadRepository readRepository,
    IParcelDeliveryOrderUpdateRepository updateRepository) : IRequestHandler<ToggleItemPickCommand, bool>
{
    public async Task<bool> Handle(ToggleItemPickCommand request, CancellationToken cancellationToken)
    {
        var order = await readRepository.GetByIdAsync(request.ItemId, cancellationToken).ConfigureAwait(false);
        var item = order?.Items?.FirstOrDefault(x => x.Id == request.ItemId);
        if (item == null) return false;

        item.IsPicked = request.IsPicked;
        updateRepository.UpdateItem(item);
        return true;
    }
}
