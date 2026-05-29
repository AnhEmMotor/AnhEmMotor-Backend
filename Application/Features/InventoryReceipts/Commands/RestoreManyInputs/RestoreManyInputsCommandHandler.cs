using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.RestoreManyInventoryReceipts;

public sealed class RestoreManyInventoryReceiptsCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreManyInventoryReceiptsCommand, Result<List<InventoryReceiptDetailResponse>>>
{
    public async Task<Result<List<InventoryReceiptDetailResponse>>> Handle(
        RestoreManyInventoryReceiptsCommand request,
        CancellationToken cancellationToken)
    {
        var InventoryReceipts = await readRepository.GetByIdAsync(request.Ids, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);
        var InventoryReceiptsList = InventoryReceipts.ToList();
        if (InventoryReceiptsList.Count != request.Ids.Count)
        {
            var foundIds = InventoryReceiptsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            return Error.NotFound(
                $"Không tìm thấy {missingIds.Count} phiếu nhập đã xóa: {string.Join(", ", missingIds)}",
                "Ids");
        }
        updateRepository.Restore(InventoryReceiptsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return InventoryReceipts.Adapt<List<InventoryReceiptDetailResponse>>();
    }
}
