using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile.MediaFile;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Infrastructure.Repositories.MediaFile.MediaFile;

public class MediaFileReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IMediaFileReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<MediaFileEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return paginator.ApplyAsync<MediaFileEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    public Task<IEnumerable<MediaFileEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<MediaFileEntity>>(t => t.Result, cancellationToken);
    }

    public Task<MediaFileEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<MediaFileEntity?> GetByStoragePathAsync(
        string storagePath,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode)
            .FirstOrDefaultAsync(f => string.Compare(f.StoragePath, storagePath) == 0, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
        ;
    }

    public Task<IEnumerable<MediaFileEntity>> GetByStoragePathsAsync(
        IEnumerable<string> storagePaths,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode)
            .Where(f => storagePaths.Contains(f.StoragePath))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<MediaFileEntity>>(t => t.Result, cancellationToken);
        ;
    }

    public Task<IEnumerable<MediaFileEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode)
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<MediaFileEntity>>(t => t.Result, cancellationToken);
        ;
    }

    internal IQueryable<MediaFileEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<MediaFileEntity>(mode);
    }
}
