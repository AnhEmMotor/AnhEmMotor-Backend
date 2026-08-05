using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListRepairOrdersForChat;

public sealed record ListRepairOrdersForChatQuery : IRequest<Result<ChatToolEnvelope<ChatRepairOrderListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
