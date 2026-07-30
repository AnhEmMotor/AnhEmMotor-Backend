using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Output;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetOrderStatusForChat;

public class GetOrderStatusForChatQueryHandler(
    IOutputReadRepository outputReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetOrderStatusForChatQuery, Result<ChatToolEnvelope<ChatOrderStatusDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatOrderStatusDto>>> Handle(
        GetOrderStatusForChatQuery request,
        CancellationToken cancellationToken)
    {
        var order = await outputReadRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken)
            .ConfigureAwait(false);
        if (order == null)
        {
            return Result<ChatToolEnvelope<ChatOrderStatusDto>>.Failure(Error.NotFound("Không tìm thấy đơn hàng"));
        }
        var dto = new ChatOrderStatusDto
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
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IOutputReadRepository.GetByIdWithDetailsAsync",
            new Dictionary<string, string>(),
            "so-don-hang",
            "VND");
        return ChatToolEnvelope<ChatOrderStatusDto>.WrapSingle(dto, meta);
    }
}
