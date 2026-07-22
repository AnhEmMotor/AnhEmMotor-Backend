using Application.ApiContracts.WarrantyTerms.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.WarrantyTerm;
using Domain.Primitives;
using MediatR;
using Mapster;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermsList;

public class GetWarrantyTermsListQueryHandler(IWarrantyTermReadRepository readRepository) 
    : IRequestHandler<GetWarrantyTermsListQuery, Result<PagedResult<WarrantyTermResponse>>>
{
    public async Task<Result<PagedResult<WarrantyTermResponse>>> Handle(GetWarrantyTermsListQuery request, CancellationToken cancellationToken)
    {
        var terms = await readRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = terms.Count;
        var items = terms
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var response = items.Adapt<List<WarrantyTermResponse>>();
        return Result<PagedResult<WarrantyTermResponse>>.Success(new PagedResult<WarrantyTermResponse>(response, totalCount, request.Page, request.PageSize));
    }
}
