using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetGa4TrafficForChat;

public sealed record GetGa4TrafficForChatQuery : IRequest<Result<ChatToolEnvelope<Ga4TrafficRowDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    /// <summary>Chiều phân rã: rỗng = tổng cả kỳ, "day" / "source" / "page" / "device".</summary>
    public string Breakdown { get; init; } = string.Empty;

    public int Limit { get; init; } = ChatToolLimit.Default;
}
