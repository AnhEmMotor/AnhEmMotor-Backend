using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetVehiclePortfolioForChat;

public sealed record GetVehiclePortfolioForChatQuery : IRequest<Result<ChatToolEnvelope<ChatVehiclePortfolioItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
