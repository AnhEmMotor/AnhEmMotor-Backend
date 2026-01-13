using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Supplier;

using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSupplierById;

public sealed class GetSupplierByIdQueryHandler(ISupplierReadRepository repository) : IRequestHandler<GetSupplierByIdQuery, Result<SupplierResponse?>>
{
    public async Task<Result<SupplierResponse?>> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var supplier = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(supplier == null)
        {
            return Error.NotFound($"Supplier with Id {request.Id} not found.");
        }

        return supplier.Adapt<SupplierResponse>();
    }
}
