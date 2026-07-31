using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetWarrantyClaimDetailForChat;

public sealed record GetWarrantyClaimDetailForChatQuery : IRequest<Result<ChatToolEnvelope<ChatWarrantyClaimDetailDto>>>
{
    public int ClaimId { get; init; }
}
