using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.BookingAppointment;
using Domain.Constants.Booking;
using Domain.Entities;
using MediatR;

namespace Application.Features.BookingAppointments.Commands;

public class CancelBookingAppointmentCommandHandler(
	IBookingAppointmentReadRepository readRepo,
	IBookingAppointmentWriteRepository writeRepo,
	IUnitOfWork uow) : IRequestHandler<CancelBookingAppointmentCommand, Result<bool>>
{
	public async Task<Result<bool>> Handle(CancelBookingAppointmentCommand req, CancellationToken ct)
	{
		var entity = await readRepo.GetByIdAsync(req.Id, ct).ConfigureAwait(false);
		if (entity is null)
			return Result<bool>.Failure(Error.NotFound("Không tìm thấy lịch hẹn."));

		entity.Status = BookingStatus.Cancelled;
		entity.CancelReason = req.CancelReason;

		writeRepo.Update(entity);
		await uow.SaveChangesAsync(ct).ConfigureAwait(false);

		return Result<bool>.Success(true);
	}
}
