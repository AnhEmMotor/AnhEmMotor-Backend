using Application.Common.Models;
using Domain.Constants;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetPartnerTypesList;

public sealed class GetPartnerTypesListQueryHandler : IRequestHandler<GetPartnerTypesListQuery, Result<List<PartnerTypeResponse>>>
{
    public Task<Result<List<PartnerTypeResponse>>> Handle(GetPartnerTypesListQuery request, CancellationToken cancellationToken)
    {
        var list = PartnerType.All.Select(key => new PartnerTypeResponse(key, PartnerType.GetName(key))).ToList();
        return Task.FromResult(Result<List<PartnerTypeResponse>>.Success(list));
    }
}
