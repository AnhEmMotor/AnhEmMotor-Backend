using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListBookingsForChat;

public sealed record ListBookingsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatBookingListItemDto>>>
{
    public int Limit { get; init; } = ChatToolLimit.Default;
}
