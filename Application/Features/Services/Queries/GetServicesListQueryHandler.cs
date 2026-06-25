using Application.ApiContracts.Service.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Service;
using Domain.Primitives;
using Mapster;
using MediatR;
using Sieve.Services;

namespace Application.Features.Services.Queries;

public class GetServicesListQueryHandler(IServiceReadRepository serviceRepository, ISieveProcessor sieveProcessor) : IRequestHandler<GetServicesListQuery, Result<PagedResult<ServiceResponse>>>
{
    public async Task<Result<PagedResult<ServiceResponse>>> Handle(
        GetServicesListQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        var query = serviceRepository.GetQueryable();
        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);
        var totalCount = filteredQuery.Count();
        var services = filteredQuery
            .Skip((request.SieveModel.Page!.Value - 1) * request.SieveModel.PageSize!.Value)
            .Take(request.SieveModel.PageSize.Value)
            .ProjectToType<ServiceResponse>()
            .ToList();
        return new PagedResult<ServiceResponse>(
            services,
            totalCount,
            request.SieveModel.Page.Value,
            request.SieveModel.PageSize.Value);
    }
}
