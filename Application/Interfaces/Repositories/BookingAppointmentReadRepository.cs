using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories;

public interface IBookingAppointmentReadRepository
{
	public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
		SieveModel sieveModel,
		DataFetchMode mode = DataFetchMode.ActiveOnly,
		Expression<Func<global::Domain.Entities.BookingAppointment, bool>>? filter = null,
		CancellationToken cancellationToken = default);

	public Task<IEnumerable<global::Domain.Entities.BookingAppointment>> GetAllAsync(
		CancellationToken cancellationToken,
		DataFetchMode mode = DataFetchMode.ActiveOnly);

	public Task<global::Domain.Entities.BookingAppointment?> GetByIdAsync(
		int id,
		CancellationToken cancellationToken,
		DataFetchMode mode = DataFetchMode.ActiveOnly);
}
