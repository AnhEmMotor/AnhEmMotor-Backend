using Application.ApiContracts.WarrantyTerms.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermsList;

public sealed record GetWarrantyTermsListQuery : IRequest<Result<PagedResult<WarrantyTermResponse>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
