using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetPnlReportForChat;

public sealed record GetPnlReportForChatQuery : IRequest<Result<ChatToolEnvelope<ChatPnlReportDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }
}
