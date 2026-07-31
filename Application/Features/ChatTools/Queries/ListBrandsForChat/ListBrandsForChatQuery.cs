using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListBrandsForChat;

public sealed record ListBrandsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatBrandListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
