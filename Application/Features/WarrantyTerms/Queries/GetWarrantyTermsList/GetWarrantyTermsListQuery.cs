using Application.ApiContracts.Admin.Warranty;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermsList;

public sealed record GetWarrantyTermsListQuery : IRequest<Result<PagedResult<WarrantyTermResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
