using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants.InventoryReceipt;
using MediatR;

namespace Application.Features.Suppliers.Commands.DeleteSupplier;

public sealed class DeleteSupplierCommandHandler(
    ISupplierReadRepository readRepository,
    ISupplierDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteSupplierCommand, Result>
{
    public async Task<Result> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (supplier == null)
        {
            return Result.Failure(Error.NotFound($"Supplier with Id {request.Id} not found."));
        }
        if (supplier.InventoryReceiptReceipts.Any(ir => string.Compare(ir.StatusId, InventoryReceiptStatus.Working) == 0))
        {
            return Result.Failure(Error.Conflict("Cannot delete supplier with working InventoryReceipt receipts."));
        }
        deleteRepository.Delete(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
