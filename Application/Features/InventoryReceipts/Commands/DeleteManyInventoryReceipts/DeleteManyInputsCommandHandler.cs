using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants.InventoryReceipt;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.DeleteManyInventoryReceipts;

public sealed class DeleteManyInventoryReceiptsCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyInventoryReceiptsCommand, Result>
{
    public async Task<Result> Handle(DeleteManyInventoryReceiptsCommand request, CancellationToken cancellationToken)
    {
        var InventoryReceipts = await readRepository.GetByIdAsync(request.Ids, cancellationToken).ConfigureAwait(false);
        var InventoryReceiptsList = InventoryReceipts.ToList();
        if (InventoryReceiptsList.Count != request.Ids.Count)
        {
            var foundIds = InventoryReceiptsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return Result.Failure(
                Error.NotFound($"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}", "Ids"));
        }
        var errors = new List<Error>();
        foreach (var output in InventoryReceiptsList)
        {
            if (InventoryReceiptStatus.IsCannotDelete(output.StatusId))
            {
                errors.Add(Error.BadRequest($"Phiếu nhập với Id {output.Id} đã bị xóa trước đó", "Ids"));
            }
        }
        if (errors.Count > 0)
        {
            return Result.Failure(errors);
        }
        deleteRepository.Delete(InventoryReceiptsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
