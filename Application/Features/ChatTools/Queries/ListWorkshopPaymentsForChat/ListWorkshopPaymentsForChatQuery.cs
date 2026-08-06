using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListWorkshopPaymentsForChat;

public sealed record ListWorkshopPaymentsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatWorkshopPaymentListItemDto>>>
{
    public int Limit { get; init; } = 10;
}
