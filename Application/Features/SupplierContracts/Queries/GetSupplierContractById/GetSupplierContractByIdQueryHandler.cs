using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierContract;
using Mapster;
using MediatR;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractById;

public class GetSupplierContractByIdQueryHandler(ISupplierContractReadRepository repository) : IRequestHandler<GetSupplierContractByIdQuery, Result<SupplierContractDetailResponse>>
{
    public async Task<Result<SupplierContractDetailResponse>> Handle(
        GetSupplierContractByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity == null)
        {
            return Result<SupplierContractDetailResponse>.Failure("Supplier contract not found.");
        }
        return entity.Adapt<SupplierContractDetailResponse>();
    }
}
