using Application.Common.Models;
using Domain.Constants.InventoryReceipt;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptStatusList;

public sealed class GetInventoryReceiptStatusListQueryHandler : IRequestHandler<GetInventoryReceiptStatusListQuery, Result<Dictionary<string, string>>>
{
    private static readonly Dictionary<string, string> Statuses = new()
    { { InventoryReceiptStatus.Working, "Phiếu tạm" }, { InventoryReceiptStatus.Finish, "Hoàn thành" }, { InventoryReceiptStatus.Cancel, "Đã huỷ" }, };

    public Task<Result<Dictionary<string, string>>> Handle(
        GetInventoryReceiptStatusListQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Result<Dictionary<string, string>>.Success(Statuses));
    }
}
