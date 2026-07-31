using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListCategoriesForChat;

public sealed record ListCategoriesForChatQuery : IRequest<Result<ChatToolEnvelope<ChatCategoryListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
