using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSalesReportForChat;

public class GetSalesReportForChatQueryHandler(
    IOutputReadRepository outputReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetSalesReportForChatQuery, Result<ChatToolEnvelope<ChatSalesReportItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatSalesReportItemDto>>> Handle(
        GetSalesReportForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var orders = await outputReadRepository.GetAllAsync(cancellationToken, DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        var filtered = orders
            .Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value >= start && o.CreatedAt.Value <= end)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = filtered
            .Take(limit)
            .Select(
                o => new ChatSalesReportItemDto
                {
                    OrderId = o.Id,
                    CustomerName = o.CustomerName,
                    StatusId = o.StatusId,
                    PaymentStatus = o.PaymentStatus,
                    Total = o.Total,
                    CreatedAt = o.CreatedAt
                })
            .ToList();
        var inner = new ChatToolResult<ChatSalesReportItemDto>(dtos, filtered.Count, filtered.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IOutputReadRepository.GetAllAsync",
            new Dictionary<string, string> { ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end) },
            "bao-cao-ban-hang",
            "VND");
        return ChatToolEnvelope<ChatSalesReportItemDto>.Wrap(inner, meta);
    }
}
