using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyClaim;
using Domain.Constants;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries;

public class GetWarrantyClaimDetailQueryHandler(
    IWarrantyClaimReadRepository readRepo) : IRequestHandler<GetWarrantyClaimDetailQuery, Result<WarrantyClaimResponse?>>
{
    public async Task<Result<WarrantyClaimResponse?>> Handle(GetWarrantyClaimDetailQuery req, CancellationToken ct)
    {
        var claim = await readRepo.GetByIdAsync(req.Id, ct, DataFetchMode.All);
        if (claim is null)
            return Result<WarrantyClaimResponse?>.Failure([Error.NotFound($"Không tìm thấy khiếu nại bảo hành id={req.Id}", "Id")]);

        var response = new WarrantyClaimResponse
        {
            Id = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            VehicleId = claim.VehicleId,
            IssueDescription = claim.IssueDescription,
            MediaUrls = claim.MediaUrls,
            ServiceCenterName = claim.ServiceCenterName,
            ManufacturerClaimNumber = claim.ManufacturerClaimNumber,
            Status = claim.Status,
            ManufacturerDecision = claim.ManufacturerDecision,
            IsRecall = claim.IsRecall,
            TotalPartsCost = claim.TotalPartsCost,
            TotalLaborCost = claim.TotalLaborCost,
            CreatedAt = claim.CreatedAt,
            UpdatedAt = claim.UpdatedAt,
            Parts = claim.WarrantyClaimParts
                .Where(p => p.DeletedAt == null)
                .Select(p => new WarrantyClaimPartResponse
                {
                    Id = p.Id,
                    WarrantyClaimId = p.WarrantyClaimId,
                    PartName = p.PartName,
                    PartCode = p.PartCode,
                    UnitPrice = p.UnitPrice,
                    Status = p.Status
                }).ToList()
        };

        return Result<WarrantyClaimResponse?>.Success(response);
    }
}
