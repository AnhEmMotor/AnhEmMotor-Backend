using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories.Supplier;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSupplierById;

public sealed class GetSupplierByIdQueryHandler(ISupplierSelectRepository repository)
    : IRequestHandler<GetSupplierByIdQuery, (SupplierResponse? Data, ErrorResponse? Error)>
{
    public async Task<(SupplierResponse? Data, ErrorResponse? Error)> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (supplier == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Supplier with Id {request.Id} not found." }]
            });
        }

        return (new SupplierResponse
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Address = supplier.Address,
            Phone = supplier.Phone,
            Email = supplier.Email,
            StatusId = supplier.StatusId
        }, null);
    }
}
