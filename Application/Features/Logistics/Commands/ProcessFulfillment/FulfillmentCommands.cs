using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Logistics;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
    private readonly IApplicationDbContext _context;

    public FulfillmentCommandsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateParcelStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.ParcelDeliveryOrders.FindAsync(new object[] { request.Id }, cancellationToken);
        if (order == null) return false;

        order.Status = request.NewStatus;
        
        // If status is shipping, maybe logic for COD can go here via MediatR events or Domain events.
        // For now just save changes.
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(UpdateTrackingNumberCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.ParcelDeliveryOrders.FindAsync(new object[] { request.Id }, cancellationToken);
        if (order == null) return false;

        order.TrackingNumber = request.TrackingNumber;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(ToggleItemPickCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ParcelDeliveryOrderItems.FindAsync(new object[] { request.ItemId }, cancellationToken);
        if (item == null) return false;

        item.IsPicked = request.IsPicked;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
