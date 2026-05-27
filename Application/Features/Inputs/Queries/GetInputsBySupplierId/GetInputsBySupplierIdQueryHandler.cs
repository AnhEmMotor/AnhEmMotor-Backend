using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputsBySupplierId;

public sealed class GetInputsBySupplierIdQueryHandler(IInputReadRepository repository) : IRequestHandler<GetInputsBySupplierIdQuery, Result<PagedResult<InputListResponse>>>
{
    public async Task<Result<PagedResult<InputListResponse>>> Handle(
        GetInputsBySupplierIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<InputListResponse>(
            request.SieveModel!,
            DataFetchMode.ActiveOnly,
            x => x.SupplierId == request.SupplierId,
            cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
