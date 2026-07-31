using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListExpensesForChat;

public sealed record ListExpensesForChatQuery : IRequest<Result<ChatToolEnvelope<ChatExpenseListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
