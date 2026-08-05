using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetInventoryReportForChat;

public sealed record GetInventoryReportForChatQuery : IRequest<Result<ChatToolEnvelope<ChatInventoryReportItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;

    public string? SearchTerm { get; init; }

    public int? Month { get; init; }

    public int? Year { get; init; }
}
