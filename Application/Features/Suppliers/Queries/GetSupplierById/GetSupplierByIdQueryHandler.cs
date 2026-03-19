using Application.ApiContracts.Supplier.Responses;
using Application.Common.Helper;
using Application.Common.Models;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSupplierById;

public sealed class GetSupplierByIdQueryHandler(ISupplierReadRepository repository) : IRequestHandler<GetSupplierByIdQuery, Result<SupplierResponse?>>
{
    public async Task<Result<SupplierResponse?>> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var supplier = await repository.GetByIdWithTotalInputAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(supplier == null)
        {
            return Error.NotFound($"Supplier with Id {request.Id} not found.");
        }

        var response = supplier.Adapt<SupplierResponse>();
        AuditColumnMapper.Apply(supplier, response, AuditColumn.CreatedAt);

        return response;
    }
}
