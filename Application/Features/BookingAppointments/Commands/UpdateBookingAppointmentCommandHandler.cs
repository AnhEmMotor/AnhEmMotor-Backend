using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.BookingAppointment;
using Domain.Entities;
using MediatR;

namespace Application.Features.BookingAppointments.Commands;

public class UpdateBookingAppointmentCommandHandler(
	IBookingAppointmentReadRepository readRepo,
	IBookingAppointmentWriteRepository writeRepo,
	IUnitOfWork uow) : IRequestHandler<UpdateBookingAppointmentCommand, Result<bool>>
{
	public async Task<Result<bool>> Handle(UpdateBookingAppointmentCommand req, CancellationToken ct)
	{
		var entity = await readRepo.GetByIdAsync(req.Id, ct).ConfigureAwait(false);
		if (entity is null)
			return Result<bool>.Failure(Error.NotFound("Không tìm thấy lịch hẹn."));

		if (req.FullName is not null) entity.FullName = req.FullName;
		if (req.Phone is not null) entity.Phone = req.Phone;
		if (req.Email is not null) entity.Email = req.Email;
		if (req.ServiceType is not null) entity.ServiceType = req.ServiceType;
		if (req.PreferredDate.HasValue) entity.PreferredDate = req.PreferredDate.Value;
		if (req.PreferredTimeSlot is not null) entity.PreferredTimeSlot = req.PreferredTimeSlot;
		if (req.AppointmentAt.HasValue) entity.AppointmentAt = req.AppointmentAt.Value;
		if (req.Showroom is not null) entity.Showroom = req.Showroom;
		if (req.Notes is not null) entity.Notes = req.Notes;

		entity.UpdatedAt = DateTimeOffset.UtcNow;

		writeRepo.Update(entity);
		await uow.SaveChangesAsync(ct).ConfigureAwait(false);

		return Result<bool>.Success(true);
	}
}
