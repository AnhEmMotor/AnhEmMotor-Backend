using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using MediatR;
using System.Linq;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers
{
    public class DeleteManySuppliersCommandHandler(
        ISupplierReadRepository readRepository,
        ISupplierDeleteRepository deleteRepository,
        ISupplierDebtRepository supplierDebtRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteManySuppliersCommand, Result>
    {
        public async Task<Result> Handle(DeleteManySuppliersCommand request, CancellationToken cancellationToken)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return Result.Success();
            }
            foreach (var id in request.Ids)
            {
                var supplier = await readRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
                if (supplier == null)
                {
                    return Result.Failure(Error.NotFound($"Supplier with Id {id} not found."));
                }
                var supplierDebts = await supplierDebtRepository.GetBySupplierIdAsync(id, cancellationToken)
                    .ConfigureAwait(false);
                if (supplierDebts.Any(
                    sd => sd.InventoryReceipt != null && InventoryReceiptStatus.IsCanEdit(sd.InventoryReceipt.StatusId)))
                {
                    return Result.Failure(
                        Error.Conflict($"Cannot delete supplier with Id {id} due to working InventoryReceipt receipts."));
                }
                deleteRepository.Delete(supplier);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
    }
}
