using Domain.Primitives;

namespace Application.ApiContracts.BookingAppointments.Responses;

public class BookingAppointmentResponse
{
	public int Id { get; set; }

	public string FullName { get; set; } = string.Empty;

	public string Phone { get; set; } = string.Empty;

	public string? Email { get; set; }

	public string? ServiceType { get; set; }

	public DateTime? PreferredDate { get; set; }

	public string? PreferredTimeSlot { get; set; }

	public DateTimeOffset? AppointmentAt { get; set; }

	public string? Showroom { get; set; }

	public string Status { get; set; } = string.Empty;

	public string? Notes { get; set; }

	public DateTimeOffset? ConfirmedAt { get; set; }

	public Guid? ConfirmedBy { get; set; }

	public string? ConfirmedByName { get; set; }

	public string? CancelReason { get; set; }

	public DateTimeOffset? CreatedAt { get; set; }

	public DateTimeOffset? UpdatedAt { get; set; }
}
