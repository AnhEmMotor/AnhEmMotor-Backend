using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSuppliersWithDebtForChat;

public sealed record GetSuppliersWithDebtForChatQuery : IRequest<Result<ChatToolEnvelope<ChatSupplierDebtListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
