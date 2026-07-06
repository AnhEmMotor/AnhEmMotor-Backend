using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyClaim;
using Domain.Primitives;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries;

public class GetWarrantyClaimsListQueryHandler(
    IWarrantyClaimReadRepository repo) : IRequestHandler<GetWarrantyClaimsListQuery, Result<PagedResult<WarrantyClaimResponse>>>
{
    public async Task<Result<PagedResult<WarrantyClaimResponse>>> Handle(GetWarrantyClaimsListQuery req, CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync<WarrantyClaimResponse>(req.Sieve, req.Mode, null, ct);
        return Result<PagedResult<WarrantyClaimResponse>>.Success(paged);
    }
}
