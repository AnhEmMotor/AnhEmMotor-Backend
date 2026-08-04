using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Booking;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListBookingsForChat;

public class ListBookingsForChatQueryHandler(IBookingReadRepository repo, IServerDateProvider dateProvider) : IRequestHandler<ListBookingsForChatQuery, Result<ChatToolEnvelope<ChatBookingListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatBookingListItemDto>>> Handle(
        ListBookingsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var all = await repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var ordered = all.OrderByDescending(x => x.PreferredDate).ToList();
        var dtos = ordered.Take(limit)
            .Select(
                x => new ChatBookingListItemDto
                {
                    BookingId = x.Id,
                    FullName = x.FullName,
                    PhoneNumber = x.PhoneNumber,
                    PreferredDate = x.PreferredDate,
                    Status = x.Status,
                    BookingType = x.BookingType,
                    Location = x.Location
                })
            .ToList();
        var totalCount = ordered.Count;
        var inner = new ChatToolResult<ChatBookingListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IBookingReadRepository.GetAllAsync",
            new Dictionary<string, string>(),
            "lich-hen-dich-vu",
            null);
        return ChatToolEnvelope<ChatBookingListItemDto>.Wrap(inner, meta);
    }
}
