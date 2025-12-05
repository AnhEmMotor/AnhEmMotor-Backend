using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreSupplier;

public sealed class RestoreSupplierCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreSupplierCommand, (SupplierResponse? Data, ErrorResponse? Error)>
{
    public async Task<(SupplierResponse? Data, ErrorResponse? Error)> Handle(
        RestoreSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(supplier == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Message = $"Deleted supplier with Id {request.Id} not found." } ]
            });
        }

        updateRepository.Restore(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (supplier.Adapt<SupplierResponse>(), null);
    }
}