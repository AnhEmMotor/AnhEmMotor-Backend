using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PlateDossier;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.PlateDossier
{
    public class PlateDossierReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IPlateDossierReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.PlateDossier, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<Domain.Entities.PlateDossier, TResponse>(
                query,
                sieveModel,
                mode,
                cancellationToken);
        }

        public Task<Domain.Entities.PlateDossier?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return context.PlateDossiers
                .Include(p => p.Output)
                .ThenInclude(o => o!.OutputInfos)
                .ThenInclude(oi => oi.ProductVariantColor)
                .ThenInclude(pvc => pvc!.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public Task<Domain.Entities.PlateDossier?> GetByOutputIdAsync(
            int outputId,
            CancellationToken cancellationToken = default)
        {
            return context.PlateDossiers
                .Include(p => p.Output)
                .FirstOrDefaultAsync(p => p.OutputId == outputId, cancellationToken);
        }

        private IQueryable<Domain.Entities.PlateDossier> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<Domain.Entities.PlateDossier>(mode)
                .Include(p => p.Output)
                .ThenInclude(o => o!.OutputInfos)
                .ThenInclude(oi => oi.ProductVariant);
        }
    }
}
