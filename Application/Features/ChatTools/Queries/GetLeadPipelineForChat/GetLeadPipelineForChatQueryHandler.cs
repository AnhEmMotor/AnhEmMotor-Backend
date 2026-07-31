using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Lead.Lead;
using Domain.Constants.Lead;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetLeadPipelineForChat;

public class GetLeadPipelineForChatQueryHandler(
    ILeadReadRepository leadReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetLeadPipelineForChatQuery, Result<ChatToolEnvelope<ChatLeadPipelineItemDto>>>
{
    private static readonly (string Key, string Display)[] Stages =
    [
        (LeadStatus.New, "Mới"),
        (LeadStatus.Consulting, "Đang tư vấn"),
        (LeadStatus.TestDriving, "Đang lái thử"),
        (LeadStatus.Deposited, "Đã đặt cọc"),
        (LeadStatus.Paperwork, "Đang chờ giấy tờ"),
        (LeadStatus.Delivered, "Đã giao xe")
    ];

    public async Task<Result<ChatToolEnvelope<ChatLeadPipelineItemDto>>> Handle(
        GetLeadPipelineForChatQuery request,
        CancellationToken cancellationToken)
    {
        var leads = await leadReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var stageOrder = Stages.Select((s, i) => (s.Key, s.Display, Order: i)).ToDictionary(s => s.Key, s => s);
        var ordered = leads
            .Where(l => stageOrder.ContainsKey(l.Status))
            .OrderBy(l => stageOrder[l.Status].Order)
            .ThenByDescending(l => l.CreatedAt)
            .ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = ordered
            .Take(limit)
            .Select(
                l => new ChatLeadPipelineItemDto
                {
                    LeadId = l.Id,
                    FullName = l.FullName,
                    PhoneNumber = l.PhoneNumber,
                    Status = l.Status,
                    StatusDisplayName = stageOrder[l.Status].Display,
                    Score = l.Score,
                    InterestedVehicle = l.InterestedVehicle,
                    CreatedAt = l.CreatedAt ?? DateTimeOffset.MinValue
                })
            .ToList();
        var inner = new ChatToolResult<ChatLeadPipelineItemDto>(dtos, ordered.Count, ordered.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ILeadReadRepository.GetAllAsync",
            new Dictionary<string, string> { ["Sắp xếp"] = "Theo giai đoạn pipeline, mới nhất trước" },
            "lead-pipeline",
            null);
        return ChatToolEnvelope<ChatLeadPipelineItemDto>.Wrap(inner, meta);
    }
}
