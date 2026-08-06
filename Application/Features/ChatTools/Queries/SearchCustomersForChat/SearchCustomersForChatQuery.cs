using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.SearchCustomersForChat;

public sealed record SearchCustomersForChatQuery : IRequest<Result<ChatToolEnvelope<ChatCustomerSearchResultDto>>>
{
    public string? Keyword { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
