using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Commands.RestoreSupplier;

public sealed class RestoreSupplierCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreSupplierCommand, Result<SupplierResponse?>>
{
    public async Task<Result<SupplierResponse?>> Handle(
        RestoreSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await readRepository.GetByIdAsync(request.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(supplier == null)
        {
            return Error.NotFound($"Deleted supplier with Id {request.Id} not found.");
        }

        updateRepository.Restore(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return supplier.Adapt<SupplierResponse>();
    }
}