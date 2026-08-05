using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetConversionToolsForChat;

public sealed record GetConversionToolsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatConversionToolDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
