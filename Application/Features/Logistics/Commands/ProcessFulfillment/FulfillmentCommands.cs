using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Commands.ProcessFulfillment;

public class UpdateParcelStatusCommand : IRequest<bool>
{
    public int Id { get; set; }
    public ParcelDeliveryStatus NewStatus { get; set; }
}

public class UpdateTrackingNumberCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
}

public class ToggleItemPickCommand : IRequest<bool>
{
    public int ItemId { get; set; }
    public bool IsPicked { get; set; }
}

public class FulfillmentCommandsHandler : 
    IRequestHandler<UpdateParcelStatusCommand, bool>,
    IRequestHandler<UpdateTrackingNumberCommand, bool>,
    IRequestHandler<ToggleItemPickCommand, bool>
{
    private readonly IParcelDeliveryOrderReadRepository _readRepository;
    private readonly IParcelDeliveryOrderUpdateRepository _updateRepository;

    public FulfillmentCommandsHandler(
        IParcelDeliveryOrderReadRepository readRepository,
        IParcelDeliveryOrderUpdateRepository updateRepository)
    {
        _readRepository = readRepository;
        _updateRepository = updateRepository;
    }

    public async Task<bool> Handle(UpdateParcelStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _readRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null) return false;

        order.Status = request.NewStatus;
        _updateRepository.Update(order);
        return true;
    }

    public async Task<bool> Handle(UpdateTrackingNumberCommand request, CancellationToken cancellationToken)
    {
        var order = await _readRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null) return false;

        order.TrackingNumber = request.TrackingNumber;
        _updateRepository.Update(order);
        return true;
    }

    public async Task<bool> Handle(ToggleItemPickCommand request, CancellationToken cancellationToken)
    {
        var order = await _readRepository.GetByIdAsync(request.ItemId, cancellationToken);
        var item = order?.Items?.FirstOrDefault(x => x.Id == request.ItemId);
        if (item == null) return false;

        item.IsPicked = request.IsPicked;
        _updateRepository.UpdateItem(item);
        return true;
    }
}
