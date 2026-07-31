using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.ConversionTool;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetConversionToolsForChat;

public class GetConversionToolsForChatQueryHandler(
    IConversionToolReadRepository conversionToolReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetConversionToolsForChatQuery, Result<ChatToolEnvelope<ChatConversionToolDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatConversionToolDto>>> Handle(
        GetConversionToolsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var tools = await conversionToolReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var ordered = tools.OrderByDescending(t => t.Views).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = ordered
            .Take(limit)
            .Select(
                t => new ChatConversionToolDto
                {
                    Id = t.Id,
                    Type = t.Type,
                    Name = t.Name,
                    IsActive = t.IsActive,
                    Views = t.Views,
                    Clicks = t.Clicks,
                    Leads = t.Leads
                })
            .ToList();
        var inner = new ChatToolResult<ChatConversionToolDto>(dtos, ordered.Count, ordered.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IConversionToolReadRepository.GetAllAsync",
            new Dictionary<string, string> { ["Sắp xếp"] = "Views giảm dần" },
            "cong-cu-chuyen-doi",
            null);
        return ChatToolEnvelope<ChatConversionToolDto>.Wrap(inner, meta);
    }
}
