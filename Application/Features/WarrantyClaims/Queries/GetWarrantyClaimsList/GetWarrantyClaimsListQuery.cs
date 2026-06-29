using Application.ApiContracts.WarrantyClaim.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.WarrantyClaims.Queries.GetWarrantyClaimsList
{
    public class GetWarrantyClaimsListQuery : IRequest<Result<PagedResult<WarrantyClaimResponse>>>
    {
        public SieveModel? SieveModel { get; set; }
    }
}
