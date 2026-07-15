using Application.ApiContracts.BookingAppointments.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.User;
using Domain.Entities;
using Domain.Primitives;
using MediatR;

namespace Application.Features.BookingAppointments.Queries;

public class GetBookingAppointmentDetailQueryHandler(
	IBookingAppointmentReadRepository repo,
	IUserReadRepository userRepo) : IRequestHandler<GetBookingAppointmentDetailQuery, Result<BookingAppointmentResponse>>
{
	public async Task<Result<BookingAppointmentResponse>> Handle(GetBookingAppointmentDetailQuery req, CancellationToken ct)
	{
		var entity = await repo.GetByIdAsync(req.Id, ct).ConfigureAwait(false);
		if (entity is null)
			return Result<BookingAppointmentResponse>.Failure(Error.NotFound("Khong tim thay lich hen."));

		var response = new BookingAppointmentResponse
		{
			Id = entity.Id,
			FullName = entity.FullName,
			Phone = entity.Phone,
			Email = entity.Email,
			ServiceType = entity.ServiceType,
			PreferredDate = entity.PreferredDate,
			PreferredTimeSlot = entity.PreferredTimeSlot,
			AppointmentAt = entity.AppointmentAt,
			Showroom = entity.Showroom,
			Status = entity.Status,
			Notes = entity.Notes,
			ConfirmedAt = entity.ConfirmedAt,
			CancelReason = entity.CancelReason,
			CreatedAt = entity.CreatedAt,
			UpdatedAt = entity.UpdatedAt,
			ConfirmedBy = entity.ConfirmedBy,
		};

		if (response.ConfirmedBy.HasValue)
		{
			var user = await userRepo.FindUserByIdAsync(response.ConfirmedBy.Value, ct).ConfigureAwait(false);
			response.ConfirmedByName = user?.FullName;
		}

		return Result<BookingAppointmentResponse>.Success(response);
	}
}
