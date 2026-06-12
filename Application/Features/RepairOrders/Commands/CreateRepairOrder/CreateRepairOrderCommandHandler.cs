using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.RepairOrder;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.RepairOrders.Commands.CreateRepairOrder
{
    public class CreateRepairOrderCommandHandler(
        IRepairOrderUpdateRepository repairOrderUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateRepairOrderCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateRepairOrderCommand request, CancellationToken cancellationToken)
        {
            var repairOrder = new RepairOrder
            {
                VehicleId = request.VehicleId,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                Mileage = request.Mileage,
                Description = request.Description,
                Status = "Pending",
                PaymentStatus = "Unpaid",
                LaborCost = 0,
                PartsCost = 0,
                TotalAmount = 0
            };

            repairOrderUpdateRepository.Add(repairOrder);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result<int>.Success(repairOrder.Id);
        }
    }
}
