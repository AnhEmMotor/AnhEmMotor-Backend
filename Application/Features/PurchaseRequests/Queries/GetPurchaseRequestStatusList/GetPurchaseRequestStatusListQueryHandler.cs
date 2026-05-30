using Application.Common.Models;
using Domain.Constants;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseRequests.Queries.GetPurchaseRequestStatusList;

public sealed class GetPurchaseRequestStatusListQueryHandler : IRequestHandler<GetPurchaseRequestStatusListQuery, Result<Dictionary<string, string>>>
{
    private static readonly Dictionary<string, string> Statuses = new()
    {
        { PurchaseRequestStatus.Draft, "Yêu cầu tạm" },
        { PurchaseRequestStatus.Sent, "Chờ phê duyệt" },
        { PurchaseRequestStatus.Approve, "Đã phê duyệt" },
        { PurchaseRequestStatus.Reject, "Bị từ chối" }
    };

    public Task<Result<Dictionary<string, string>>> Handle(
        GetPurchaseRequestStatusListQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Result<Dictionary<string, string>>.Success(Statuses));
    }
}
