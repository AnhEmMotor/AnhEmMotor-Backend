using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListSupplierContractsForChat;

public sealed record ListSupplierContractsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatSupplierContractListItemDto>>>
{
    public string? StatusId { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
