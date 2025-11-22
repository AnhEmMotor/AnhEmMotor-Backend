using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.UpdateManyProductStatuses;

public sealed class UpdateManyProductStatusesCommandHandler(
    IProductSelectRepository selectRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateManyProductStatusesCommand, (List<int>? Data, ErrorResponse? Error)>
{
    public async Task<(List<int>? Data, ErrorResponse? Error)> Handle(UpdateManyProductStatusesCommand command, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var ids = command.Ids.Distinct().ToList();

        // First query to check which IDs exist
        var existingIds = await selectRepository.GetActiveProducts()
            .Where(p => ids.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var id in ids)
        {
            if (!existingIds.Contains(id))
            {
                errors.Add(new ErrorDetail
                {
                    Field = $"Id: {id}",
                    Message = $"Sản phẩm với Id {id} không tồn tại."
                });
            }
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        // Fetch entities WITHOUT navigation properties and with AsNoTracking to avoid tracking conflicts
        var productEntities = await selectRepository.GetActiveProducts()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new Domain.Entities.Product
            {
                Id = p.Id,
                Name = p.Name,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                StatusId = p.StatusId,
                Weight = p.Weight,
                Dimensions = p.Dimensions,
                Wheelbase = p.Wheelbase,
                SeatHeight = p.SeatHeight,
                GroundClearance = p.GroundClearance,
                FuelCapacity = p.FuelCapacity,
                TireSize = p.TireSize,
                FrontSuspension = p.FrontSuspension,
                RearSuspension = p.RearSuspension,
                EngineType = p.EngineType,
                MaxPower = p.MaxPower,
                OilCapacity = p.OilCapacity,
                FuelConsumption = p.FuelConsumption,
                TransmissionType = p.TransmissionType,
                StarterSystem = p.StarterSystem,
                MaxTorque = p.MaxTorque,
                Displacement = p.Displacement,
                BoreStroke = p.BoreStroke,
                CompressionRatio = p.CompressionRatio,
                Description = p.Description
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var product in productEntities)
        {
            product.StatusId = command.StatusId;
            updateRepository.Update(product);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (ids, null);
    }
}
