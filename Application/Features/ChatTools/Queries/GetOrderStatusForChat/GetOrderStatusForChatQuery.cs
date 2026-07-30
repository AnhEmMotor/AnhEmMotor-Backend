using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetOrderStatusForChat;

public sealed record GetOrderStatusForChatQuery : IRequest<Result<ChatToolEnvelope<ChatOrderStatusDto>>>
{
    public int OrderId { get; init; }
}
