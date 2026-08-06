using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetLeadPipelineForChat;

public sealed record GetLeadPipelineForChatQuery : IRequest<Result<ChatToolEnvelope<ChatLeadPipelineItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
