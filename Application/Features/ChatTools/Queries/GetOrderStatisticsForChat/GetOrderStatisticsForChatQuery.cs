using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetOrderStatisticsForChat;

public sealed record GetOrderStatisticsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatOrderStatisticsDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }
}
