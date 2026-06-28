using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using System;

namespace Application.Features.Logistics.Returns.Commands.RejectReturn;

public class RejectReturnCommandHandler(
    IParcelDeliveryOrderReadRepository readRepo,
    IParcelDeliveryOrderUpdateRepository updateRepo,
    IUnitOfWork unitOfWork) : IRequestHandler<RejectReturnCommand, bool>
{
    public async Task<bool> Handle(RejectReturnCommand request, CancellationToken cancellationToken)
    {
        var order = await readRepo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (order == null || order.Status != ParcelDeliveryStatus.Returned)
            return false;

        order.RejectionReason = request.RejectionReason;
        order.ReturnAction = "rejected";
        order.InspectedAt = DateTime.UtcNow;
        updateRepo.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
