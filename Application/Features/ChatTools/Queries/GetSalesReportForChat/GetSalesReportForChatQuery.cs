using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSalesReportForChat;

public sealed record GetSalesReportForChatQuery : IRequest<Result<ChatToolEnvelope<ChatSalesReportItemDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
