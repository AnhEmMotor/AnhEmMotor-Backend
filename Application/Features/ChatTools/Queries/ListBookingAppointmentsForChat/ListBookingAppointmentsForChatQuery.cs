using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListBookingAppointmentsForChat;

public sealed record ListBookingAppointmentsForChatQuery
    : IRequest<Result<ChatToolEnvelope<ChatBookingAppointmentListItemDto>>>
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
