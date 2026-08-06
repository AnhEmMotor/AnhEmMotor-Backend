using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListVouchersForChat;

public sealed record ListVouchersForChatQuery : IRequest<Result<ChatToolEnvelope<ChatVoucherListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
