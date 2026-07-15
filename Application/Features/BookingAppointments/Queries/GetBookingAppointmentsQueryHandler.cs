using Application.ApiContracts.BookingAppointments.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.User;
using Domain.Primitives;
using MediatR;

namespace Application.Features.BookingAppointments.Queries;

public class GetBookingAppointmentsQueryHandler(
	IBookingAppointmentReadRepository repo,
	IUserReadRepository userRepo) : IRequestHandler<GetBookingAppointmentsQuery, Result<PagedResult<BookingAppointmentResponse>>>
{
	public async Task<Result<PagedResult<BookingAppointmentResponse>>> Handle(
		GetBookingAppointmentsQuery req,
		CancellationToken ct)
	{
		var paged = await repo.GetPagedAsync<BookingAppointmentResponse>(req.Sieve, req.Mode, null, ct);

		if (paged.Items is not null && paged.Items.Count > 0)
		{
			var distinctUserIds = paged.Items
				.Where(x => x.ConfirmedBy.HasValue)
				.Select(x => x.ConfirmedBy.Value)
				.Distinct()
				.ToList();

			foreach (var userId in distinctUserIds)
			{
				var user = await userRepo.FindUserByIdAsync(userId, ct).ConfigureAwait(false);
				var name = user?.FullName;
				foreach (var item in paged.Items.Where(x => x.ConfirmedBy == userId))
					item.ConfirmedByName = name;
			}
		}

		return Result<PagedResult<BookingAppointmentResponse>>.Success(paged);
	}
}
