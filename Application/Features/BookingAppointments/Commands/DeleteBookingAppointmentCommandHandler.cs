using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.BookingAppointment;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.BookingAppointments.Commands;

public class DeleteBookingAppointmentCommandHandler(
	IBookingAppointmentReadRepository readRepo,
	IBookingAppointmentWriteRepository writeRepo,
	IUnitOfWork uow) : IRequestHandler<DeleteBookingAppointmentCommand, Result<bool>>
{
	public async Task<Result<bool>> Handle(DeleteBookingAppointmentCommand req, CancellationToken ct)
	{
		var entity = await readRepo.GetByIdAsync(req.Id, ct, mode: DataFetchMode.All).ConfigureAwait(false);
		if (entity is null)
			return Result<bool>.Failure(Error.NotFound("Không tìm thấy lịch hẹn."));

		writeRepo.Delete(entity);
		await uow.SaveChangesAsync(ct).ConfigureAwait(false);

		return Result<bool>.Success(true);
	}
}
