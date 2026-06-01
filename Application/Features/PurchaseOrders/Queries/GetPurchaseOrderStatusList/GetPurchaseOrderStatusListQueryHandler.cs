using Application.Common.Models;
using Domain.Constants;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseOrders.Queries.GetPurchaseOrderStatusList
{
    public sealed class GetPurchaseOrderStatusListQueryHandler : IRequestHandler<GetPurchaseOrderStatusListQuery, Result<Dictionary<string, string>>>
    {
        private static readonly Dictionary<string, string> Statuses = new()
        {
            { PurchaseOrderStatus.Draft, "Đơn mua tạm" },
            { PurchaseOrderStatus.Sent, "Đã gửi / Chờ duyệt" },
            { PurchaseOrderStatus.Approved, "Đã phê duyệt" },
            { PurchaseOrderStatus.Rejected, "Bị từ chối" }
        };

        public Task<Result<Dictionary<string, string>>> Handle(
            GetPurchaseOrderStatusListQuery request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Result<Dictionary<string, string>>.Success(Statuses));
        }
    }
}
