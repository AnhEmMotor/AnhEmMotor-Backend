using Application.Common.Models;
using Domain.Constants.Order;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputStatusList;

public sealed class GetOutputStatusListQueryHandler : IRequestHandler<GetOutputStatusListQuery, Result<Dictionary<string, string>>>
{
    private static readonly Dictionary<string, string> Statuses = new()
    {
        { OrderStatus.Pending, "Chờ xác nhận" },
        { OrderStatus.ConfirmedCod, "Xác nhận COD" },
        { OrderStatus.PaidProcessing, "Đã thanh toán - Đang xử lý" },
        { OrderStatus.WaitingDeposit, "Chờ đặt cọc" },
        { OrderStatus.DepositPaid, "Đã đặt cọc" },
        { OrderStatus.Delivering, "Đang giao" },
        { OrderStatus.WaitingPickup, "Chờ nhận hàng" },
        { OrderStatus.Completed, "Đã hoàn thành" },
        { OrderStatus.Refunding, "Đang hoàn tiền" },
        { OrderStatus.Refunded, "Đã hoàn tiền" },
        { OrderStatus.Cancelled, "Đã huỷ" },
    };

    public Task<Result<Dictionary<string, string>>> Handle(
        GetOutputStatusListQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Result<Dictionary<string, string>>.Success(Statuses));
    }
}
