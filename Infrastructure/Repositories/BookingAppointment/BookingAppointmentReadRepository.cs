using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.BookingAppointment;

public class BookingAppointmentReadRepository(
	ApplicationDBContext context,
	ISievePaginator paginator) : IBookingAppointmentReadRepository
{
	public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
		SieveModel sieveModel,
		DataFetchMode mode = DataFetchMode.ActiveOnly,
		Expression<Func<global::Domain.Entities.BookingAppointment, bool>>? filter = null,
		CancellationToken cancellationToken = default)
	{
		var query = GetQueryable(mode);
		if (filter != null)
			query = query.Where(filter);

		return paginator.ApplyAsync<global::Domain.Entities.BookingAppointment, TResponse>(query, sieveModel, mode, cancellationToken);
	}

	private IQueryable<global::Domain.Entities.BookingAppointment> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
	{
		var query = context.Set<global::Domain.Entities.BookingAppointment>().IgnoreQueryFilters();

		if (mode == DataFetchMode.ActiveOnly)
			query = query.Where(x => x.DeletedAt == null);
		else if (mode == DataFetchMode.DeletedOnly)
			query = query.Where(x => x.DeletedAt != null);

		return query.AsNoTracking();
	}

	public Task<IEnumerable<global::Domain.Entities.BookingAppointment>> GetAllAsync(
		CancellationToken cancellationToken,
		DataFetchMode mode = DataFetchMode.ActiveOnly)
	{
		return GetQueryable(mode)
			.ToListAsync(cancellationToken)
			.ContinueWith<IEnumerable<global::Domain.Entities.BookingAppointment>>(t => t.Result, cancellationToken);
	}

	public Task<global::Domain.Entities.BookingAppointment?> GetByIdAsync(
		int id,
		CancellationToken cancellationToken,
		DataFetchMode mode = DataFetchMode.ActiveOnly)
	{
		return GetQueryable(mode)
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
			.ContinueWith(t => t.Result, cancellationToken);
	}
}
