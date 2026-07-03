using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Maintenances.Commands.UpdateOdo
{
    public class UpdateOdoCommandHandler(
        IVehicleReadRepository vehicleReadRepository,
        IVehicleUpdateRepository vehicleUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateOdoCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateOdoCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await vehicleReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (vehicle == null)
            {
                return Result<bool>.Failure("Không tìm thấy phương tiện.");
            }

            vehicle.CurrentOdo = request.CurrentOdo;
            vehicleUpdateRepository.Update(vehicle);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
    }
}
