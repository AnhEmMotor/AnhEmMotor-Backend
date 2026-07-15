using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.BookingAppointment;
using Domain.Constants.Booking;
using Domain.Entities;
using MediatR;

namespace Application.Features.BookingAppointments.Commands;

public class ConfirmBookingAppointmentCommandHandler(
	IBookingAppointmentReadRepository readRepo,
	IBookingAppointmentWriteRepository writeRepo,
	IUnitOfWork uow) : IRequestHandler<ConfirmBookingAppointmentCommand, Result<bool>>
{
	public async Task<Result<bool>> Handle(ConfirmBookingAppointmentCommand req, CancellationToken ct)
	{
		var entity = await readRepo.GetByIdAsync(req.Id, ct).ConfigureAwait(false);
		if (entity is null)
			return Result<bool>.Failure(Error.NotFound("Không tìm thấy lịch hẹn."));

		entity.Status = BookingStatus.Confirmed;
		entity.ConfirmedAt = DateTimeOffset.UtcNow;
		entity.ConfirmedBy = req.CurrentUserId;
		if (req.AppointmentAt.HasValue)
			entity.AppointmentAt = req.AppointmentAt;

		writeRepo.Update(entity);
		await uow.SaveChangesAsync(ct).ConfigureAwait(false);

		return Result<bool>.Success(true);
	}
}
