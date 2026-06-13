using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Logistics.Returns.Commands.InspectReturn
{
    public class InspectReturnCommandHandler(
    IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository,
    IParcelDeliveryOrderUpdateRepository parcelDeliveryOrderUpdateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<InspectReturnCommand, bool>
    {
        public async Task<bool> Handle(InspectReturnCommand request, CancellationToken cancellationToken)
        {
            var order = await parcelDeliveryOrderReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

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
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}
