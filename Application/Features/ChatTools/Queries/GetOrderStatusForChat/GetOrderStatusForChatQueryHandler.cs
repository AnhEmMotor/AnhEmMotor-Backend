using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Output;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetOrderStatusForChat;

public class GetOrderStatusForChatQueryHandler(
    IOutputReadRepository outputReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetOrderStatusForChatQuery, Result<ChatToolEnvelope<ChatOrderStatusDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatOrderStatusDto>>> Handle(
        GetOrderStatusForChatQuery request,
        CancellationToken cancellationToken)
    {
        var keyword = request.Keyword.Trim();
        var orderIds = await ChatToolOrderSearch
            .FindOrderIdsByKeywordAsync(outputReadRepository, keyword, cancellationToken)
            .ConfigureAwait(false);
        var dtos = new List<ChatOrderStatusDto>();
        foreach (var orderId in orderIds)
        {
            var order = await outputReadRepository.GetByIdWithDetailsAsync(orderId, cancellationToken)
                .ConfigureAwait(false);
            if (order == null)
            {
                continue;
            }
            dtos.Add(
                new ChatOrderStatusDto
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
                });
        }
        var inner = new ChatToolResult<ChatOrderStatusDto>(dtos, dtos.Count, false);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IOutputReadRepository.GetPagedAsync+GetByIdWithDetailsAsync",
            new Dictionary<string, string> { ["Từ khóa"] = keyword },
            "so-don-hang",
            "VND");
        return ChatToolEnvelope<ChatOrderStatusDto>.Wrap(inner, meta);
    }
}
