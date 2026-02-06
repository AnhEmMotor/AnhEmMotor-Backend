using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Suppliers.Commands.CreateSupplier;

public sealed class CreateSupplierCommandHandler(
    ISupplierReadRepository supplierReadRepository,
    ISupplierInsertRepository supplierInsertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateSupplierCommand, Result<SupplierResponse>>
{
    public async Task<Result<SupplierResponse>> Handle(
        CreateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var isDuplicate = await supplierReadRepository.IsNameExistsAsync(request.Name!, null, cancellationToken)
            .ConfigureAwait(false);
        if(isDuplicate)
        {
            return Result<SupplierResponse>.Failure("Supplier name already exists.");
        }

        if(!string.IsNullOrWhiteSpace(request.Phone))
        {
            var isPhoneDuplicate = await supplierReadRepository.IsPhoneExistsAsync(
                request.Phone,
                null,
                cancellationToken)
                .ConfigureAwait(false);
            if(isPhoneDuplicate)
            {
                return Result<SupplierResponse>.Failure("Supplier phone already exists.");
            }
        }

        if(!string.IsNullOrWhiteSpace(request.TaxIdentificationNumber))
        {
            var isTaxIdDuplicate = await supplierReadRepository.IsTaxIdExistsAsync(
                request.TaxIdentificationNumber,
                null,
                cancellationToken)
                .ConfigureAwait(false);
            if(isTaxIdDuplicate)
            {
                return Result<SupplierResponse>.Failure("Tax Identification Number already exists.");
            }
        }

        var supplier = request.Adapt<Supplier>();
        supplierInsertRepository.Add(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return supplier.Adapt<SupplierResponse>();
    }
}
