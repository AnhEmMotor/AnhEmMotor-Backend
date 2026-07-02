using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.Bookings.Commands.UpdateBooking
{
    public class UpdateBookingCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTimeOffset PreferredDate { get; set; }
        public int? ProductVariantId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string BookingType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
