using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.SearchSuppliersForChat;

public sealed record SearchSuppliersForChatQuery : IRequest<Result<ChatToolEnvelope<ChatSupplierSearchResultDto>>>
{
    public string? Keyword { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
