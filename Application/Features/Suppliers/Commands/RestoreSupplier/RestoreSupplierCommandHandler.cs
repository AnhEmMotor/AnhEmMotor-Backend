using Application.ApiContracts.Supplier.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Common.Models;
using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreSupplier;

public sealed class RestoreSupplierCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreSupplierCommand, (SupplierResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(SupplierResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        RestoreSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(supplier == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors =
                    [ new Common.Models.ErrorDetail { Message = $"Deleted supplier with Id {request.Id} not found." } ]
            });
        }

        updateRepository.Restore(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (supplier.Adapt<SupplierResponse>(), null);
    }
}