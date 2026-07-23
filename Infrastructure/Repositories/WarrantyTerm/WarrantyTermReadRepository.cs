using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyTerm;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.WarrantyTerm;

public class WarrantyTermReadRepository(
	ApplicationDBContext context,
	ISievePaginator paginator) : IWarrantyTermReadRepository
{
	public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
		SieveModel sieveModel,
		DataFetchMode mode = DataFetchMode.ActiveOnly,
		Expression<Func<global::Domain.Entities.WarrantyTerm, bool>>? filter = null,
		CancellationToken cancellationToken = default)
	{
		var query = GetQueryable(mode);
		if (filter != null)
			query = query.Where(filter);
		return paginator.ApplyAsync<global::Domain.Entities.WarrantyTerm, TResponse>(query, sieveModel, mode, cancellationToken);
	}

	internal IQueryable<global::Domain.Entities.WarrantyTerm> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
	{
		var query = context.Set<global::Domain.Entities.WarrantyTerm>().IgnoreQueryFilters();
		if (mode == DataFetchMode.ActiveOnly)
			query = query.Where(x => x.DeletedAt == null);
		else if (mode == DataFetchMode.DeletedOnly)
			query = query.Where(x => x.DeletedAt != null);
		return query.AsNoTracking();
	}

	public Task<IEnumerable<global::Domain.Entities.WarrantyTerm>> GetAllAsync(
		CancellationToken cancellationToken,
		Func<IQueryable<global::Domain.Entities.WarrantyTerm>, IQueryable<global::Domain.Entities.WarrantyTerm>>? include = null,
		DataFetchMode mode = DataFetchMode.ActiveOnly)
	{
		var query = GetQueryable(mode);
		if (include != null)
			query = include(query);
		return query.ToListAsync(cancellationToken)
			.ContinueWith<IEnumerable<global::Domain.Entities.WarrantyTerm>>(t => t.Result, cancellationToken);
	}

	public Task<global::Domain.Entities.WarrantyTerm?> GetByIdAsync(
		int id,
		CancellationToken cancellationToken,
		Func<IQueryable<global::Domain.Entities.WarrantyTerm>, IQueryable<global::Domain.Entities.WarrantyTerm>>? include = null,
		DataFetchMode mode = DataFetchMode.ActiveOnly)
	{
		var query = GetQueryable(mode);
		if (include != null)
			query = include(query);
		return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
			.ContinueWith(t => t.Result, cancellationToken);
	}

	public async Task<global::Application.ApiContracts.Admin.Warranty.WarrantyTermStatisticsResponse> GetStatisticsAsync(
		CancellationToken cancellationToken)
	{
		var query = GetQueryable(DataFetchMode.ActiveOnly);

		var totalTerms = await query.CountAsync(cancellationToken).ConfigureAwait(false);
		var activeTerms = await query.CountAsync(t => t.Status == "Active", cancellationToken).ConfigureAwait(false);
		var inactiveTerms = await query.CountAsync(t => t.Status == "Inactive", cancellationToken).ConfigureAwait(false);
		var expiredTerms = await query.CountAsync(t => t.Status == "Expired", cancellationToken).ConfigureAwait(false);

		var brandsCovered = await query
			.Select(t => t.BrandId)
			.Distinct()
			.CountAsync(cancellationToken)
			.ConfigureAwait(false);

		return new global::Application.ApiContracts.Admin.Warranty.WarrantyTermStatisticsResponse
		{
			TotalTerms = totalTerms,
			ActiveTerms = activeTerms,
			InactiveTerms = inactiveTerms + expiredTerms,
			BrandsCovered = brandsCovered
		};
	}
	}
}
