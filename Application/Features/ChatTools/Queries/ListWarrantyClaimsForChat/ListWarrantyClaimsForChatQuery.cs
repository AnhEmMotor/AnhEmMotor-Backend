using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListWarrantyClaimsForChat;

public sealed record ListWarrantyClaimsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatWarrantyClaimListItemDto>>>
{
    public string? StatusId { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
