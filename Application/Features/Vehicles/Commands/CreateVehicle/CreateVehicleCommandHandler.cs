using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Vehicles.Commands.CreateVehicle;

public sealed class CreateVehicleCommandHandler(
    IVehicleReadRepository readRepository,
    IVehicleUpdateRepository updateRepository,
    ILeadReadRepository leadReadRepository,
    IProductReadRepository productReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateVehicleCommand, Result<VehicleResponse?>>
{
    public async Task<Result<VehicleResponse?>> Handle(
        CreateVehicleCommand request,
        CancellationToken cancellationToken)
    {
        var leadExists = await leadReadRepository.GetQueryable()
            .AnyAsync(l => l.Id == request.LeadId, cancellationToken)
            .ConfigureAwait(false);
        if (!leadExists)
        {
            return Result<VehicleResponse?>.Failure(
                Error.NotFound($"Lead with ID {request.LeadId} not found.", "LeadId"));
        }
        var productExists = await productReadRepository.GetQueryable(DataFetchMode.All)
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken)
            .ConfigureAwait(false);
        if (!productExists)
        {
            return Result<VehicleResponse?>.Failure(
                Error.NotFound($"Product with ID {request.ProductId} not found.", "ProductId"));
        }
        if (string.IsNullOrWhiteSpace(request.VinNumber))
        {
            return Result<VehicleResponse?>.Failure(Error.BadRequest("VIN cannot be empty.", "VinNumber"));
        }
        var isVinExists = await readRepository.GetQuery(DataFetchMode.All)
            .AnyAsync(v => string.Compare(v.VinNumber, request.VinNumber.Trim()) == 0, cancellationToken)
            .ConfigureAwait(false);
        if (isVinExists)
        {
            return Result<VehicleResponse?>.Failure(Error.BadRequest("VIN already exists.", "VinNumber"));
        }
        var isEngineExists = await readRepository.GetQuery(DataFetchMode.All)
            .AnyAsync(v => string.Compare(v.EngineNumber, request.EngineNumber.Trim()) == 0, cancellationToken)
            .ConfigureAwait(false);
        if (isEngineExists)
        {
            return Result<VehicleResponse?>.Failure(Error.BadRequest("Engine number already exists.", "EngineNumber"));
        }
        var vehicle = new Vehicle
        {
            LeadId = request.LeadId,
            ProductId = request.ProductId,
            VinNumber = request.VinNumber.Trim(),
            EngineNumber = request.EngineNumber.Trim(),
            LicensePlate = request.LicensePlate?.Trim() ?? string.Empty,
            PurchaseDate = request.PurchaseDate ?? DateTimeOffset.UtcNow,
            IsActive = true
        };
        updateRepository.Add(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return vehicle.Adapt<VehicleResponse>();
    }
}
