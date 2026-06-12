using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities.Logistics;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Logistics.Returns.Commands.InspectReturn;

public class InspectReturnCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string? BoxCondition { get; set; }
    public string? ProductCondition { get; set; }
    public string? ReturnProofImage { get; set; }
    public string? ReturnInternalNote { get; set; }
    public string Action { get; set; } = string.Empty; // "restock", "defect", "refund"
}

public class InspectReturnCommandHandler(IApplicationDbContext db)
    : IRequestHandler<InspectReturnCommand, bool>
{
    public async Task<bool> Handle(InspectReturnCommand request, CancellationToken cancellationToken)
    {
        var order = await db.ParcelDeliveryOrders
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.Status == ParcelDeliveryStatus.Returned, cancellationToken);

        if (order == null) return false;

        order.InspectedAt = DateTime.UtcNow;
        order.BoxCondition = request.BoxCondition;
        order.ProductCondition = request.ProductCondition;
        order.ReturnProofImage = request.ReturnProofImage;
        order.ReturnInternalNote = request.ReturnInternalNote;
        order.ReturnAction = request.Action;

        // In a real application, you would add logic here to:
        // - Update inventory (add back stock for "restock", move to defect storage for "defect")
        // - Trigger refund logic to finance module for "refund"
        
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
