using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetDebtLogsMissingProofsForChat;

public sealed record GetDebtLogsMissingProofsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatDebtLogMissingProofItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
