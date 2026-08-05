using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetDashboardOverviewForChat;

public sealed record GetDashboardOverviewForChatQuery : IRequest<Result<ChatToolEnvelope<ChatDashboardOverviewDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }
}
