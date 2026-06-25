using Application.ApiContracts.Client.Bookings;
using Application.Interfaces.Repositories.ServiceBooking;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Client.Bookings
{
    public record GetAvailableSlotsQuery(DateTime Date) : IRequest<List<AvailableSlotResponse>>;

    public record CreateBookingCommand(CreateBookingRequest Request) : IRequest<int>;

    public record GetBookingHistoryQuery() : IRequest<List<BookingHistoryResponse>>;

    public record CancelBookingCommand(int Id, string Reason) : IRequest<bool>;

    public class GetAvailableSlotsHandler : IRequestHandler<GetAvailableSlotsQuery, List<AvailableSlotResponse>>
    {
        public async Task<List<AvailableSlotResponse>> Handle(
            GetAvailableSlotsQuery request,
            CancellationToken cancellationToken)
        {
            var slots = new List<AvailableSlotResponse>();
            for (int i = 8; i < 17; i++)
                slots.Add(new AvailableSlotResponse(request.Date.AddHours(i), request.Date.AddHours(i + 1), true));
            return await Task.FromResult(slots);
        }
    }

    public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, int>
    {
        private readonly IServiceBookingInsertRepository _insertRepo;

        public CreateBookingHandler(IServiceBookingInsertRepository insertRepo) => _insertRepo = insertRepo;

        public async Task<int> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = new ServiceBooking
            {
                VehicleId = request.Request.VehicleId,
                ServiceId = int.TryParse(request.Request.ServiceType, out var sid) ? sid : 1,
                ScheduledDate = request.Request.AppointmentDate.Add(request.Request.AppointmentTime),
                Notes = request.Request.Notes,
                Status = BookingServiceStatus.Pending.ToString()
            };
            return await _insertRepo.AddAsync(booking, cancellationToken);
        }
    }

    public class GetBookingHistoryHandler : IRequestHandler<GetBookingHistoryQuery, List<BookingHistoryResponse>>
    {
        private readonly IServiceBookingReadRepository _readRepo;

        public GetBookingHistoryHandler(IServiceBookingReadRepository readRepo) => _readRepo = readRepo;

        public async Task<List<BookingHistoryResponse>> Handle(
            GetBookingHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var bookings = await _readRepo.GetAllAsync(cancellationToken);
            return bookings.Select(
                b => new BookingHistoryResponse(
                    b.Id,
                    b.ScheduledDate.DateTime,
                    b.ServiceId.ToString(),
                    b.Status,
                    b.Status))
                .ToList();
        }
    }

    public class CancelBookingHandler : IRequestHandler<CancelBookingCommand, bool>
    {
        private readonly IServiceBookingUpdateRepository _updateRepo;

        public CancelBookingHandler(IServiceBookingUpdateRepository updateRepo) => _updateRepo = updateRepo;

        public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            return await _updateRepo.UpdateStatusAsync(
                request.Id,
                BookingStatus.Cancelled.ToString(),
                request.Reason,
                DateTime.UtcNow,
                cancellationToken);
        }
    }
}
