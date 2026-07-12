using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries;

public class GetWarrantyHistoryQueryHandler(IWarrantyClaimReadRepository repo) : IRequestHandler<GetWarrantyHistoryQuery, Result<IEnumerable<WarrantyHistoryResponse>>>
{
    public async Task<Result<IEnumerable<WarrantyHistoryResponse>>> Handle(
        GetWarrantyHistoryQuery req,
        CancellationToken ct)
    {
        var claims = await repo.GetHistoryByVehicleIdAsync(req.VehicleId, ct, DataFetchMode.All).ConfigureAwait(false);
        var response = claims.Select(Map).ToList();
        return Result<IEnumerable<WarrantyHistoryResponse>>.Success(response);
    }

    private static WarrantyHistoryResponse Map(WarrantyClaim c)
    {
        return new WarrantyHistoryResponse
        {
            Id = c.Id,
            ClaimNumber = c.ClaimNumber,
            Status = c.Status,
            StatusText = WarrantyClaimStatus.GetLabel(c.Status),
            IssueDescription = c.IssueDescription,
            ManufacturerDecision = c.ManufacturerDecision,
            IsRecall = c.IsRecall,
            TotalPartsCost = c.TotalPartsCost,
            TotalLaborCost = c.TotalLaborCost,
            CreatedAt = c.CreatedAt.GetValueOrDefault(),
            Parts =
                c.WarrantyClaimParts
                    .Where(p => p.DeletedAt == null)
                    .Select(
                        p => new WarrantyClaimPartResponse
                    {
                        Id = p.Id,
                        WarrantyClaimId = p.WarrantyClaimId,
                        PartName = p.PartName,
                        PartCode = p.PartCode,
                        UnitPrice = p.UnitPrice,
                        Status = p.Status
                    })
                    .ToList()
        };
    }
}
