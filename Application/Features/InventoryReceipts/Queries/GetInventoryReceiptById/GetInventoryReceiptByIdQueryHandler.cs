using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;

using Mapster;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptById;

public class GetInventoryReceiptByIdQueryHandler(IInventoryReceiptReadRepository repository) : IRequestHandler<GetInventoryReceiptByIdQuery, Result<InventoryReceiptDetailResponse?>>
{
    public async Task<Result<InventoryReceiptDetailResponse?>> Handle(
        GetInventoryReceiptByIdQuery request,
        CancellationToken cancellationToken)
    {
        var InventoryReceipt = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (InventoryReceipt is null)
        {
            return Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.");
        }
        return InventoryReceipt.Adapt<InventoryReceiptDetailResponse>();
    }
}
