using Application.ApiContracts.Brand.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand;

public class BrandReadRepository(
    ApplicationDBContext context,
    ISievePaginator paginator,
    ISieveProcessor sieveProcessor) : IBrandReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        return paginator.ApplyAsync<BrandEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    public Task<List<BrandEntity>> GetFilteredListAsync(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        var filteredQuery = sieveProcessor.Apply(sieveModel, query, applyPagination: false);
        return filteredQuery.ToListAsync(cancellationToken);
    }

    public Task<IEnumerable<BrandEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<BrandEntity>(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<BrandEntity>>(t => t.Result, cancellationToken);
    }

    public Task<BrandEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<BrandEntity>(mode)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<BrandEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<BrandEntity>(mode)
            .Where(b => ids.Contains(b.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<BrandEntity>>(t => t.Result, cancellationToken);
    }

    public Task<List<BrandEntity>> GetByNameAsync(
        string name,
        CancellationToken cancellationToken,
        DataFetchMode dataFetchMode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<BrandEntity>(dataFetchMode).Where(b => b.Name == name).ToListAsync(cancellationToken);
    }

    public async Task<BrandStatisticsResponse> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var query = context.GetQuery<BrandEntity>(DataFetchMode.ActiveOnly);
        var totalBrands = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var deletedBrandsCount = await context.GetQuery<BrandEntity>(DataFetchMode.DeletedOnly)
            .CountAsync(cancellationToken).ConfigureAwait(false);
        var popularOriginGroup = await query
            .Where(b => !string.IsNullOrEmpty(b.Origin))
            .GroupBy(b => b.Origin)
            .Select(g => new { Origin = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        var latestUpdatedBrand = await query
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Select(b => new { b.Name, LatestTime = b.UpdatedAt ?? b.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        return new BrandStatisticsResponse
        {
            TotalBrands = totalBrands,
            PopularOrigin = popularOriginGroup?.Origin,
            PopularOriginCount = popularOriginGroup?.Count ?? 0,
            LatestUpdatedBrandName = latestUpdatedBrand?.Name,
            LatestUpdatedAt = latestUpdatedBrand?.LatestTime,
            DeletedBrandsCount = deletedBrandsCount
        };
    }

    internal IQueryable<BrandEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<BrandEntity>(mode);
    }
}
