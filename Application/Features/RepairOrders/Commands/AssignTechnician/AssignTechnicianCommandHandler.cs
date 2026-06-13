using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.RepairOrder;
using MediatR;

namespace Application.Features.RepairOrders.Commands.AssignTechnician
{
    public class AssignTechnicianCommandHandler(
        IRepairOrderReadRepository repairOrderReadRepository,
        IRepairOrderUpdateRepository repairOrderUpdateRepository,
        IEmployeeReadRepository employeeReadRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<AssignTechnicianCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(AssignTechnicianCommand request, CancellationToken cancellationToken)
        {
            var repairOrder = await repairOrderReadRepository.GetByIdAsync(request.RepairOrderId, cancellationToken)
                .ConfigureAwait(false);
            if (repairOrder == null)
            {
                return Result<bool>.Failure(Error.NotFound("Phiếu sửa chữa không tồn tại."));
            }
            var technician = await employeeReadRepository.GetByIdAsync(request.TechnicianId, cancellationToken)
                .ConfigureAwait(false);
            if (technician == null)
            {
                return Result<bool>.Failure(Error.NotFound("Kỹ thuật viên không tồn tại."));
            }
            repairOrder.TechnicianId = request.TechnicianId;
            repairOrder.Status = "InProgress";
            repairOrderUpdateRepository.Update(repairOrder);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<bool>.Success(true);
        }
    }
}
