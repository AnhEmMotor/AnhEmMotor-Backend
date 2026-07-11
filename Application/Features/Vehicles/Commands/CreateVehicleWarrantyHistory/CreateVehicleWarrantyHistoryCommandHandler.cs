using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using MediatR;

namespace Application.Features.Vehicles.Commands.CreateVehicleWarrantyHistory;

public class CreateVehicleWarrantyHistoryCommandHandler(
    IVehicleReadRepository vehicleReadRepository,
    IVehicleWarrantyHistoryWriteRepository writeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateVehicleWarrantyHistoryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateVehicleWarrantyHistoryCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleReadRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            return Result<int>.Failure([Error.NotFound("Vehicle not found.", "VehicleId")]);
        }

        var entity = new VehicleWarrantyHistory
        {
            VehicleId = vehicle.Id,
            UserId = request.UserId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ProviderName = request.ProviderName,
            PolicyNumber = request.PolicyNumber,
            Description = request.Description,
            Status = request.Status,
            CoverageAmount = request.CoverageAmount,
            CreatedAt = DateTimeOffset.UtcNow
        };

        writeRepository.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}
