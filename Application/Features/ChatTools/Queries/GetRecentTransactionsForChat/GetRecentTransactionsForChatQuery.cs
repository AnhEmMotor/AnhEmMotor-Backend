using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetRecentTransactionsForChat;

public sealed record GetRecentTransactionsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatRecentTransactionDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
