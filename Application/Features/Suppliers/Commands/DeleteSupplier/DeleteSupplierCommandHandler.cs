using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Suppliers.Commands.DeleteSupplier
{
    public class DeleteSupplierCommandHandler(
        ISupplierReadRepository readRepository,
        ISupplierDeleteRepository deleteRepository,
        ISupplierDebtRepository supplierDebtRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteSupplierCommand, Result>
    {
        public async Task<Result> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplier = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (supplier == null)
            {
                return Result.Failure(Error.NotFound($"Supplier with Id {request.Id} not found."));
            }
            var supplierDebts = await supplierDebtRepository.GetBySupplierIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (supplierDebts.Any(
                sd => sd.InventoryReceipt != null && InventoryReceiptStatus.IsCanEdit(sd.InventoryReceipt.StatusId)))
            {
                return Result.Failure(Error.Conflict("Cannot delete supplier with working InventoryReceipt receipts."));
            }
            deleteRepository.Delete(supplier);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }
}
