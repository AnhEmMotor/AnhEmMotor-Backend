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

		if (req.VehicleId.HasValue) claim.VehicleId = req.VehicleId.Value;
		if (req.IssueDescription is not null) claim.IssueDescription = req.IssueDescription;
		if (req.MediaUrls is not null) claim.MediaUrls = req.MediaUrls;
		if (req.ServiceCenterName is not null) claim.ServiceCenterName = req.ServiceCenterName;
		if (req.ManufacturerClaimNumber is not null) claim.ManufacturerClaimNumber = req.ManufacturerClaimNumber;
		if (req.Status.HasValue) claim.Status = req.Status.Value;
		if (req.ManufacturerDecision is not null) claim.ManufacturerDecision = req.ManufacturerDecision;
		if (req.IsRecall.HasValue) claim.IsRecall = req.IsRecall.Value;
		if (req.TotalPartsCost.HasValue) claim.TotalPartsCost = req.TotalPartsCost.Value;
		if (req.TotalLaborCost.HasValue) claim.TotalLaborCost = req.TotalLaborCost.Value;

		writeRepo.Update(claim);
		await uow.SaveChangesAsync(ct);
		return Result.Success();
	}
}
