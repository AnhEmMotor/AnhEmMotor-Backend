using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Repositories.Product;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.ApiContracts.Vehicle.Responses;

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
        // Check if Lead exists
        var leadExists = await leadReadRepository.GetQueryable()
            .AnyAsync(l => l.Id == request.LeadId, cancellationToken)
            .ConfigureAwait(false);
        if (!leadExists)
        {
            return Result<VehicleResponse?>.Failure(Error.NotFound($"Lead with ID {request.LeadId} not found.", "LeadId"));
        }

        // Check if Product exists
        var productExists = await productReadRepository.GetQueryable(DataFetchMode.All)
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken)
            .ConfigureAwait(false);
        if (!productExists)
        {
            return Result<VehicleResponse?>.Failure(Error.NotFound($"Product with ID {request.ProductId} not found.", "ProductId"));
        }

        // VAS_008: VIN cannot be empty (should be handled by validator, but also here)
        if (string.IsNullOrWhiteSpace(request.VinNumber))
        {
            return Result<VehicleResponse?>.Failure(Error.BadRequest("VIN cannot be empty.", "VinNumber"));
        }

        // VAS_002: Check VIN duplicate
        var isVinExists = await readRepository.GetQuery(DataFetchMode.All)
            .AnyAsync(v => string.Compare(v.VinNumber, request.VinNumber.Trim()) == 0, cancellationToken)
            .ConfigureAwait(false);
        if (isVinExists)
        {
            return Result<VehicleResponse?>.Failure(Error.BadRequest("VIN already exists.", "VinNumber"));
        }

        // VAS_003: Check EngineNumber duplicate
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
            IsActive = true // VAS_001: Default status is Active
        };

        updateRepository.Add(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return vehicle.Adapt<VehicleResponse>();
    }
}
