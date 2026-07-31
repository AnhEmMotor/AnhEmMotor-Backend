using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetLeadDetailForChat;

public sealed record GetLeadDetailForChatQuery : IRequest<Result<ChatToolEnvelope<ChatLeadDetailDto>>>
{
    public required string Keyword { get; init; }
}
