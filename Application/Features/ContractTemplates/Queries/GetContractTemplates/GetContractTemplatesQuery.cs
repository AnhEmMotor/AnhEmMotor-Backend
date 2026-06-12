using Application.ApiContracts.ContractTemplate.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.ContractTemplates.Queries.GetContractTemplates;

public sealed record GetContractTemplatesQuery : IRequest<Result<PagedResult<ContractTemplateResponse>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public string? Sorts { get; init; }
    public string? Filters { get; init; }

    public static GetContractTemplatesQuery FromRequest(SieveModel request)
    {
        return new GetContractTemplatesQuery
        {
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 10,
            Search = request.Filters, // Simplification for now
            Sorts = request.Sorts,
            Filters = request.Filters
        };
    }
}
