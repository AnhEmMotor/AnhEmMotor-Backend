using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;

namespace Application.Features.Vehicles.Commands.UpdateLicensePlate
{
    public class UpdateLicensePlateCommandHandler(
        IVehicleReadRepository readRepository,
        IVehicleUpdateRepository updateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateLicensePlateCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateLicensePlateCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (vehicle is null)
            {
                return Result<bool>.Failure(Error.NotFound($"Vehicle with ID {request.Id} not found.", "Id"));
            }
            vehicle.LicensePlate = request.LicensePlate?.Trim() ?? string.Empty;
            updateRepository.Update(vehicle);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<bool>.Success(true);
        }
    }
}
