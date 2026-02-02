using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using SupplierEntity = Domain.Entities.Supplier;
using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateSupplierCommand, Result<SupplierResponse?>>
{
    public async Task<Result<SupplierResponse?>> Handle(
    UpdateSupplierCommand request,
    CancellationToken cancellationToken)
    {
        var supplier = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (supplier == null)
        {
            return Error.NotFound($"Supplier with Id {request.Id} not found.");
        }

        var isNameExists = await readRepository.IsNameExistsAsync(request.Name!, request.Id, cancellationToken);
        if (isNameExists)
        {
            return Error.Conflict("Supplier name already exists.");
        }

        var isPhoneExists = await readRepository.IsPhoneExistsAsync(request.Phone!, request.Id, cancellationToken);
        if (isPhoneExists)
        {
            return Error.Conflict("Supplier phone already exists.");
        }

        var isTaxIdExists = await readRepository.IsTaxIdExistsAsync(request.TaxIdentificationNumber!, request.Id, cancellationToken);
        if (isTaxIdExists)
        {
            return Error.Conflict("Supplier Tax ID already exists.");
        }

        var config = new TypeAdapterConfig();
        config.NewConfig<UpdateSupplierCommand, SupplierEntity>()
              .IgnoreNullValues(true);

        request.Adapt(supplier, config);

        updateRepository.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return supplier.Adapt<SupplierResponse>();
    }
}