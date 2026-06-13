using Application.Common.Models;
using Domain.Constants;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptStatusList;

public class GetInventoryReceiptStatusListQueryHandler : IRequestHandler<GetInventoryReceiptStatusListQuery, Result<Dictionary<string, string>>>
{
    private static readonly Dictionary<string, string> Statuses = new()
    {
        { InventoryReceiptStatus.Draft, "Phiếu tạm" },
        { InventoryReceiptStatus.Sent, "Đã gửi" },
        { InventoryReceiptStatus.Approve, "Đã duyệt" },
        { InventoryReceiptStatus.Reject, "Đã từ chối" }
    };

    public Task<Result<Dictionary<string, string>>> Handle(
        GetInventoryReceiptStatusListQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Result<Dictionary<string, string>>.Success(Statuses));
    }
}
