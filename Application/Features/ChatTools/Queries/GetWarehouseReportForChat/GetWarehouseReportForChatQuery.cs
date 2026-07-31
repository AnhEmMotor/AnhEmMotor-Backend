using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetWarehouseReportForChat;

public sealed record GetWarehouseReportForChatQuery : IRequest<Result<ChatToolEnvelope<ChatWarehouseReportDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }
}
