using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetRevenueByCategoryForChat;

public class GetRevenueByCategoryForChatQueryHandler(
    IStatisticalReadRepository statisticalReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetRevenueByCategoryForChatQuery, Result<ChatToolEnvelope<ChatRevenueByCategoryItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatRevenueByCategoryItemDto>>> Handle(
        GetRevenueByCategoryForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var categories = await statisticalReadRepository.GetRevenueByCategoryAsync(start, end, cancellationToken)
            .ConfigureAwait(false);
        var categoryList = categories.OrderByDescending(c => c.Revenue).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = categoryList
            .Take(limit)
            .Select(
                c => new ChatRevenueByCategoryItemDto
                {
                    CategoryName = c.CategoryName,
                    Revenue = c.Revenue,
                    Percentage = c.Percentage
                })
            .ToList();
        var inner = new ChatToolResult<ChatRevenueByCategoryItemDto>(
            dtos,
            categoryList.Count,
            categoryList.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalReadRepository.GetRevenueByCategoryAsync",
            new Dictionary<string, string> { ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end) },
            "doanh-thu-theo-danh-muc",
            "VND");
        return ChatToolEnvelope<ChatRevenueByCategoryItemDto>.Wrap(inner, meta);
    }
}
