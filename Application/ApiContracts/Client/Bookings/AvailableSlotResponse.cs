using System;

namespace Application.ApiContracts.Client.Bookings
{
    public record AvailableSlotResponse(DateTime SlotStart, DateTime SlotEnd, bool IsAvailable);

    public record CreateBookingRequest(
        int VehicleId,
        string ServiceType,
        DateTime AppointmentDate,
        TimeSpan AppointmentTime,
        string Notes);

    public record BookingHistoryResponse(
        int Id,
        DateTime AppointmentDate,
        string ServiceType,
        string Status,
        string StatusDescription);

    public record CancelBookingRequest(string Reason);
}
