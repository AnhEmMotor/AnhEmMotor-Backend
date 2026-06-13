using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.RepairOrder;
using MediatR;
using System;

namespace Application.Features.RepairOrders.Commands.CompleteRepairOrder
{
    public class CompleteRepairOrderCommandHandler(
        IRepairOrderReadRepository repairOrderReadRepository,
        IRepairOrderUpdateRepository repairOrderUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CompleteRepairOrderCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(CompleteRepairOrderCommand request, CancellationToken cancellationToken)
        {
            var repairOrder = await repairOrderReadRepository.GetByIdAsync(request.RepairOrderId, cancellationToken)
                .ConfigureAwait(false);
            if (repairOrder == null)
            {
                return Result<bool>.Failure(Error.NotFound("Phiếu sửa chữa không tồn tại."));
            }
            repairOrder.Status = "Completed";
            repairOrder.PaymentStatus = request.PaymentStatus;
            repairOrder.PaymentMethod = request.PaymentMethod;
            repairOrder.CompletedDate = DateTimeOffset.UtcNow;
            if (!string.IsNullOrEmpty(request.Notes))
            {
                repairOrder.Notes = request.Notes;
            }
            repairOrderUpdateRepository.Update(repairOrder);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<bool>.Success(true);
        }
    }
}
