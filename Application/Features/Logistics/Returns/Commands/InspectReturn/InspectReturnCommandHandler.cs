using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Enums;
using MediatR;
using System;

namespace Application.Features.Logistics.Returns.Commands.InspectReturn
{
    public class InspectReturnCommandHandler(
        IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository,
        IParcelDeliveryOrderUpdateRepository parcelDeliveryOrderUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<InspectReturnCommand, bool>
    {
        public async Task<bool> Handle(InspectReturnCommand request, CancellationToken cancellationToken)
        {
            var order = await parcelDeliveryOrderReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (order == null || order.Status != ParcelDeliveryStatus.Returned)
                return false;
            order.InspectedAt = DateTime.UtcNow;
            order.BoxCondition = request.BoxCondition;
            order.ProductCondition = request.ProductCondition;
            order.ReturnProofImage = request.ReturnProofImage;
            order.ReturnInternalNote = request.ReturnInternalNote;
            order.ReturnAction = string.IsNullOrWhiteSpace(request.Action) ? null : request.Action;
            parcelDeliveryOrderUpdateRepository.Update(order);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
    }
}
