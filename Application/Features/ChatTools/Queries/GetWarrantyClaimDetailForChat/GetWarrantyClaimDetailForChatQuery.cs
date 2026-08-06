using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetWarrantyClaimDetailForChat;

public sealed record GetWarrantyClaimDetailForChatQuery : IRequest<Result<ChatToolEnvelope<ChatWarrantyClaimDetailDto>>>
{
    public required string Keyword { get; init; }
}
