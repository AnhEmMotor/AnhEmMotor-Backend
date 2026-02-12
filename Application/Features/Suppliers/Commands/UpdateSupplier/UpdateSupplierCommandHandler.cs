using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Mapster;
using MediatR;
using SupplierEntity = Domain.Entities.Supplier;

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

        if(supplier == null)
        {
            return Error.NotFound($"Supplier with Id {request.Id} not found.");
        }

        if(!string.IsNullOrWhiteSpace(request.Name))
        {
            var isNameExists = await readRepository.IsNameExistsAsync(request.Name, request.Id, cancellationToken)
                .ConfigureAwait(false);
            if(isNameExists)
            {
                return Error.Conflict("Supplier name already exists.");
            }
        }

        if(!string.IsNullOrWhiteSpace(request.Phone))
        {
            var isPhoneExists = await readRepository.IsPhoneExistsAsync(request.Phone, request.Id, cancellationToken)
                .ConfigureAwait(false);
            if(isPhoneExists)
            {
                return Error.Conflict("Supplier phone already exists.");
            }
        }

        if(!string.IsNullOrWhiteSpace(request.Email))
        {
            var isEmailExists = await readRepository.IsEmailExistsAsync(request.Email, request.Id, cancellationToken)
                .ConfigureAwait(false);
            if(isEmailExists)
            {
                return Error.Conflict("Supplier email already exists.");
            }
        }

        if(!string.IsNullOrWhiteSpace(request.TaxIdentificationNumber))
        {
            var isTaxIdExists = await readRepository.IsTaxIdExistsAsync(
                request.TaxIdentificationNumber,
                request.Id,
                cancellationToken)
                .ConfigureAwait(false);
            if(isTaxIdExists)
            {
                return Error.Conflict("Supplier Tax ID already exists.");
            }
        }

        var config = new TypeAdapterConfig();
        config.NewConfig<UpdateSupplierCommand, SupplierEntity>().IgnoreNullValues(true);

        request.Adapt(supplier, config);

        updateRepository.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return supplier.Adapt<SupplierResponse>();
    }
}