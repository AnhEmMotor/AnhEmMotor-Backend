namespace Application.Features.ChatTools.Queries.ListBookingsForChat;

public sealed record ChatBookingListItemDto
{
    public int BookingId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTimeOffset PreferredDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string BookingType { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
}
