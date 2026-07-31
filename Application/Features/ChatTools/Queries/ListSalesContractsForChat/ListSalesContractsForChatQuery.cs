using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListSalesContractsForChat;

public sealed record ListSalesContractsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatSalesContractListItemDto>>>
{
    public string? StatusId { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
