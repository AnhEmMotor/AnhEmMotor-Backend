using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetLogisticsDashboardForChat;

public sealed record GetLogisticsDashboardForChatQuery : IRequest<Result<ChatToolEnvelope<ChatLogisticsDashboardDto>>>
{
    /// <summary>
    /// "today" | "month" | "year" — mặc định "today".
    /// </summary>
    public string Range { get; init; } = "today";
}
