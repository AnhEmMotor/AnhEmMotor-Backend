using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.BookingAppointment;
using Domain.Constants.Booking;
using Domain.Entities;
using MediatR;

namespace Application.Features.BookingAppointments.Commands;

public class CreateBookingAppointmentCommandHandler(IBookingAppointmentWriteRepository writeRepo, IUnitOfWork uow) : IRequestHandler<CreateBookingAppointmentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateBookingAppointmentCommand req, CancellationToken ct)
    {
        var entity = new BookingAppointment
        {
            FullName = req.FullName,
            Phone = req.Phone,
            Email = req.Email,
            ServiceType = req.ServiceType,
            PreferredDate = req.PreferredDate,
            PreferredTimeSlot = req.PreferredTimeSlot,
            AppointmentAt = req.AppointmentAt,
            Showroom = req.Showroom,
            Status = BookingStatus.Pending,
            Notes = req.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        writeRepo.Add(entity);
        await uow.SaveChangesAsync(ct).ConfigureAwait(false);
        return Result<int>.Success(entity.Id);
    }
}
