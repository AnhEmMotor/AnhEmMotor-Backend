using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using MediatR;

namespace Application.Features.Vehicles.Commands.CreateVehiclePurchaseHistory;

public class CreateVehiclePurchaseHistoryCommandHandler(
    IVehicleReadRepository vehicleReadRepository,
    IVehiclePurchaseHistoryWriteRepository writeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateVehiclePurchaseHistoryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateVehiclePurchaseHistoryCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleReadRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            return Result<int>.Failure([Error.NotFound("Vehicle not found.", "VehicleId")]);
        }

        var entity = new VehiclePurchaseHistory
        {
            VehicleId = vehicle.Id,
            UserId = request.UserId,
            PurchaseDate = request.PurchaseDate,
            InvoiceNumber = request.InvoiceNumber,
            Amount = request.Amount,
            SellerName = request.SellerName,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow
        };

        writeRepository.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}
