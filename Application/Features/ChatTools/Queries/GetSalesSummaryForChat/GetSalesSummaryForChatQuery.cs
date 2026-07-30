using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSalesSummaryForChat;

public sealed record GetSalesSummaryForChatQuery : IRequest<Result<ChatToolEnvelope<ChatDailyRevenueDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
