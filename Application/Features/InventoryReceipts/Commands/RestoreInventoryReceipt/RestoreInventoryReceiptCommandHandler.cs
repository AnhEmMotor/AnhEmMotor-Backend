using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.RestoreInventoryReceipt;

public sealed class RestoreInventoryReceiptCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreInventoryReceiptCommand, Result<InventoryReceiptDetailResponse>>
{
    public async Task<Result<InventoryReceiptDetailResponse>> Handle(
        RestoreInventoryReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var InventoryReceipt = await readRepository.GetByIdAsync(
            request.Id!.Value,
            cancellationToken,
            DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);
        if (InventoryReceipt is null)
        {
            return Error.NotFound($"Kh�ng t�m th?y phi?u nh?p d� x�a c� ID {request.Id}.", "Id");
        }
        updateRepository.Restore(InventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return InventoryReceipt.Adapt<InventoryReceiptDetailResponse>();
    }
}

