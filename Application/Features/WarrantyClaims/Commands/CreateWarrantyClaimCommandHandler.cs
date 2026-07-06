using System.Collections.Generic;
using System.Linq;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Repositories.WarrantyClaim;
using Domain.Entities;
using MediatR;

namespace Application.Features.WarrantyClaims.Commands;

public class CreateWarrantyClaimCommandHandler(
    IVehicleReadRepository vehicleRepo,
    IWarrantyClaimWriteRepository writeRepo,
    IWarrantyClaimReadRepository readRepo,
    IUnitOfWork uow) : IRequestHandler<CreateWarrantyClaimCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateWarrantyClaimCommand req, CancellationToken ct)
    {
        var vehicle = await vehicleRepo.GetByIdAsync(req.VehicleId, ct);
        if (vehicle is null)
            return Result<int>.Failure([Error.BadRequest("Xe không tồn tại.", "VehicleId")]);

        var claimNumber = $"WC-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var claim = new WarrantyClaim
        {
            ClaimNumber = claimNumber,
            VehicleId = req.VehicleId,
            IssueDescription = req.IssueDescription,
            MediaUrls = req.MediaUrls,
            ServiceCenterName = req.ServiceCenterName,
            Status = 0,
            IsRecall = req.IsRecall,
            TotalPartsCost = req.TotalPartsCost,
            TotalLaborCost = req.TotalLaborCost,
            CreatedAt = DateTimeOffset.UtcNow
        };

        writeRepo.Add(claim);
        await uow.SaveChangesAsync(ct);

        if (req.Parts is { Count: > 0 })
        {
            var parts = req.Parts.Select(p => new WarrantyClaimPart
            {
                WarrantyClaimId = claim.Id,
                PartName = p.PartName,
                PartCode = p.PartCode,
                UnitPrice = p.UnitPrice,
                Status = p.Status,
                CreatedAt = DateTimeOffset.UtcNow
            }).ToList();

            foreach (var part in parts)
                writeRepo.AddPart(part);

            await uow.SaveChangesAsync(ct);
        }

        return Result<int>.Success(claim.Id);
    }
}
