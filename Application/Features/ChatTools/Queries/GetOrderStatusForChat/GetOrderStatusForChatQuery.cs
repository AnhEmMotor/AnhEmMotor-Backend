using Application.Common.Models;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetOrderStatusForChat;

public sealed record GetOrderStatusForChatQuery : IRequest<Result<ChatOrderStatusDto>>
{
    public int OrderId { get; init; }
}
