using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using MediatR;

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

public class InspectReturnCommandHandler(
    IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository,
    IParcelDeliveryOrderUpdateRepository parcelDeliveryOrderUpdateRepository,
    Application.Interfaces.Repositories.IUnitOfWork unitOfWork)
    : IRequestHandler<InspectReturnCommand, bool>
{
    public async Task<bool> Handle(InspectReturnCommand request, CancellationToken cancellationToken)
    {
        var order = await parcelDeliveryOrderReadRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order == null || order.Status != ParcelDeliveryStatus.Returned) return false;

        order.InspectedAt = DateTime.UtcNow;
        order.BoxCondition = request.BoxCondition;
        order.ProductCondition = request.ProductCondition;
        order.ReturnProofImage = request.ReturnProofImage;
        order.ReturnInternalNote = request.ReturnInternalNote;
        order.ReturnAction = request.Action;

        // In a real application, you would add logic here to:
        // - Update inventory (add back stock for "restock", move to defect storage for "defect")
        // - Trigger refund logic to finance module for "refund"
        
        parcelDeliveryOrderUpdateRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
