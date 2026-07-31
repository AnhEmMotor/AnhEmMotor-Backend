using Application.ApiContracts.BookingAppointments.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories;
using Domain.Constants;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListBookingAppointmentsForChat;

public class ListBookingAppointmentsForChatQueryHandler(
    IBookingAppointmentReadRepository repo,
    IServerDateProvider dateProvider)
    : IRequestHandler<ListBookingAppointmentsForChatQuery, Result<ChatToolEnvelope<ChatBookingAppointmentListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatBookingAppointmentListItemDto>>> Handle(
        ListBookingAppointmentsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel
        {
            Sorts = "-AppointmentAt",
            Page = 1,
            PageSize = limit
        };

        var paged = await repo
            .GetPagedAsync<BookingAppointmentResponse>(
                sieveModel,
                DataFetchMode.ActiveOnly,
                x => x.AppointmentAt >= start && x.AppointmentAt <= end,
                cancellationToken)
            .ConfigureAwait(false);

        var items = paged.Items ?? [];
        var dtos = items
            .Select(
                x => new ChatBookingAppointmentListItemDto
                {
                    AppointmentId = x.Id,
                    FullName = x.FullName,
                    Phone = x.Phone,
                    ServiceType = x.ServiceType,
                    AppointmentAt = x.AppointmentAt,
                    Status = x.Status,
                    Showroom = x.Showroom
                })
            .ToList();

        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatBookingAppointmentListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IBookingAppointmentReadRepository.GetPagedAsync",
            new Dictionary<string, string> { ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end) },
            "lich-hen",
            null);

        return ChatToolEnvelope<ChatBookingAppointmentListItemDto>.Wrap(inner, meta);
    }
}
