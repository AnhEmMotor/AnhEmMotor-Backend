using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSupplierDebtDetailForChat;

public sealed record GetSupplierDebtDetailForChatQuery : IRequest<Result<ChatToolEnvelope<ChatSupplierDebtDetailDto>>>
{
    public int SupplierId { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
