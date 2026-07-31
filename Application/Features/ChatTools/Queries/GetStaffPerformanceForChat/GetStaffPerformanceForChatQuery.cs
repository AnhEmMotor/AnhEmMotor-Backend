using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetStaffPerformanceForChat;

public sealed record GetStaffPerformanceForChatQuery : IRequest<Result<ChatToolEnvelope<ChatStaffPerformanceItemDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
