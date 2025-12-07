using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetInputsBySupplierId;

public sealed class GetInputsBySupplierIdQueryHandler(IInputReadRepository repository, IPaginator paginator) : IRequestHandler<GetInputsBySupplierIdQuery, Domain.Primitives.PagedResult<InputResponse>>
{
    public Task<Domain.Primitives.PagedResult<InputResponse>> Handle(
        GetInputsBySupplierIdQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetBySupplierIdAsync(request.SupplierId, cancellationToken);

        return paginator.ApplyAsync<InputEntity, InputResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}
