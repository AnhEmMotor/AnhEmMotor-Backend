namespace Application.Features.ChatTools.Queries.ListBookingAppointmentsForChat;

public sealed record ChatBookingAppointmentListItemDto
{
    public int AppointmentId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string? ServiceType { get; init; }

    public DateTimeOffset? AppointmentAt { get; init; }

    public string Status { get; init; } = string.Empty;

    public string? Showroom { get; init; }
}
