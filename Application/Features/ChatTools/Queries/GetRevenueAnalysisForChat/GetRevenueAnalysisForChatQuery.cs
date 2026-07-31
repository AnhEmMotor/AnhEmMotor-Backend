using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetRevenueAnalysisForChat;

public sealed record GetRevenueAnalysisForChatQuery : IRequest<Result<ChatToolEnvelope<ChatRevenueAnalysisDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }
}
