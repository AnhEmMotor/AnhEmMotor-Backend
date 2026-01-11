using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Primitives;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetInputsBySupplierId;

public sealed class GetInputsBySupplierIdQueryHandler(IInputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetInputsBySupplierIdQuery, Result<PagedResult<InputResponse>>>
{
    public async Task<Result<PagedResult<InputResponse>>> Handle(
        GetInputsBySupplierIdQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetBySupplierIdAsync(request.SupplierId, cancellationToken);

        return await paginator.ApplyAsync<InputEntity, InputResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}
