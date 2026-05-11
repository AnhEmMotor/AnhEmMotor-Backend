using Application.Common.Models;
using Domain.Primitives;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.OptionValue;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Infrastructure.Repositories.OptionValue
{
    public class OptionValueReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IOptionValueReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<OptionValueEntity, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<OptionValueEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }
        public Task<List<OptionValueEntity>> GetByIdAsync(List<int> optionValueIds, CancellationToken cancellationToken)
        {
            return context.OptionValues
                .Include(ov => ov.Option)
                .Where(ov => optionValueIds.Contains(ov.Id))
                .ToListAsync(cancellationToken);
        }

        public Task<OptionValueEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return context.OptionValues
                .Include(ov => ov.Option)
                .FirstOrDefaultAsync(ov => ov.Id == id, cancellationToken);
        }

        public Task<OptionValueEntity?> GetByIdAndNameAsync(
            int optionId,
            string name,
            CancellationToken cancellationToken)
        {
            return context.OptionValues
                .FirstOrDefaultAsync(
                    ov => ov.OptionId == optionId &&
                        ov.Name != null &&
                        string.Compare(ov.Name.ToLower(), name.ToLower()) == 0,
                    cancellationToken);
        }

        public Task<List<OptionValueEntity>> GetByIdAndNameAsync(
            List<int> optionIds,
            List<string> names,
            CancellationToken cancellationToken)
        {
            var lowerNames = names.Select(n => n.ToLower()).ToList();
            return GetQueryable()
                .Where(
                    ov => ov.OptionId.HasValue &&
                        ov.Name != null &&
                        optionIds.Contains(ov.OptionId.Value) &&
                        lowerNames.Contains(ov.Name.ToLower()))
                .ToListAsync(cancellationToken)
                .ContinueWith(t => t.Result, cancellationToken);
        }

        internal IQueryable<OptionValueEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<OptionValueEntity>(mode);
        }
    }
}
