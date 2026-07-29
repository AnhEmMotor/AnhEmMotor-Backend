using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetOrderStatusForChat;

public class GetOrderStatusForChatQueryHandler(IOutputReadRepository outputReadRepository)
    : IRequestHandler<GetOrderStatusForChatQuery, Result<ChatOrderStatusDto>>
{
    public async Task<Result<ChatOrderStatusDto>> Handle(
        GetOrderStatusForChatQuery request,
        CancellationToken cancellationToken)
    {
        var order = await outputReadRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken)
            .ConfigureAwait(false);
        if (order == null)
        {
            return Result<ChatOrderStatusDto>.Failure(Error.NotFound("Không tìm thấy đơn hàng"));
        }
        return new ChatOrderStatusDto
        {
            OrderId = order.Id,
            StatusId = order.StatusId,
            CustomerName = order.CustomerName,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            Total = order.Total,
            PaidAmount = order.PaidAmount,
            CreatedAt = order.CreatedAt,
            LastStatusChangedAt = order.LastStatusChangedAt
        };
    }
}
