using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListServicesForChat;

public sealed record ListServicesForChatQuery : IRequest<Result<ChatToolEnvelope<ChatServiceListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
