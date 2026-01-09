using Application.ApiContracts.Supplier.Responses;
using Application.Interfaces.Repositories.Supplier;

using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSupplierById;

public sealed class GetSupplierByIdQueryHandler(ISupplierReadRepository repository) : IRequestHandler<GetSupplierByIdQuery, (SupplierResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(SupplierResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var supplier = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(supplier == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors = [ new Common.Models.ErrorDetail { Message = $"Supplier with Id {request.Id} not found." } ]
            });
        }

        return (supplier.Adapt<SupplierResponse>(), null);
    }
}
