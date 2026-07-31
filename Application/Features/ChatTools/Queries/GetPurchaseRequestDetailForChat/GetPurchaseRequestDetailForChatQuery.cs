using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetPurchaseRequestDetailForChat;

public sealed record GetPurchaseRequestDetailForChatQuery : IRequest<Result<ChatToolEnvelope<ChatPurchaseRequestDetailDto>>>
{
    /// <summary>Tên nhà cung cấp (hoặc một phần tên) — không phải ID, người dùng cuối không biết ID.</summary>
    public required string Keyword { get; init; }
}
