using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyClaim;
using MediatR;

namespace Application.Features.WarrantyClaims.Commands;

public class UpdateWarrantyClaimCommandHandler(
    IWarrantyClaimReadRepository readRepo,
    IWarrantyClaimWriteRepository writeRepo,
    IUnitOfWork uow) : IRequestHandler<UpdateWarrantyClaimCommand, Result>
{
    public async Task<Result> Handle(UpdateWarrantyClaimCommand req, CancellationToken ct)
    {
        var claim = await readRepo.GetByIdAsync(req.Id, ct);
        if (claim is null)
            return Result.Failure([Error.NotFound($"Không tìm thấy khiếu nại bảo hành id={req.Id}", "Id")]);

        claim.VehicleId = req.VehicleId;
        claim.IssueDescription = req.IssueDescription;
        claim.MediaUrls = req.MediaUrls;
        claim.ServiceCenterName = req.ServiceCenterName;
        claim.ManufacturerClaimNumber = req.ManufacturerClaimNumber;
        claim.Status = req.Status;
        claim.ManufacturerDecision = req.ManufacturerDecision;
        claim.IsRecall = req.IsRecall;
        claim.TotalPartsCost = req.TotalPartsCost;
        claim.TotalLaborCost = req.TotalLaborCost;

        writeRepo.Update(claim);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
