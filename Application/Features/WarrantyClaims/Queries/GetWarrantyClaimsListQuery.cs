using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.WarrantyClaims.Queries;

public class GetWarrantyClaimsListQuery : IRequest<Result<PagedResult<WarrantyClaimResponse>>>
{
    public SieveModel Sieve { get; set; } = new();

    public DataFetchMode Mode { get; set; } = DataFetchMode.ActiveOnly;
}
