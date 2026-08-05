using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListFinanceContractsForChat;

public sealed record ListFinanceContractsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatFinanceContractListItemDto>>>
{
    public string? StatusId { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
