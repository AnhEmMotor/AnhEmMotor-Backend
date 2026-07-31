using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetWorkshopDashboardForChat;

public sealed record GetWorkshopDashboardForChatQuery : IRequest<Result<ChatToolEnvelope<ChatWorkshopDashboardDto>>>
{
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}
