using Application.Interfaces.Repositories.Option;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using OptionEntity = Domain.Entities.Option;

namespace Infrastructure.Repositories.Option
{
    public class OptionReadRepository(ApplicationDBContext context) : IOptionReadRepository
    {
        public IQueryable<OptionEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        { return context.GetQuery<OptionEntity>(mode); }

        public Task<IEnumerable<OptionEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return GetQueryable(mode)
                .ToListAsync(cancellationToken)
                .ContinueWith<IEnumerable<OptionEntity>>(t => t.Result, cancellationToken);
        }

        public Task<OptionEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return GetQueryable(mode)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
                .ContinueWith(t => t.Result, cancellationToken);
        }

        public Task<IEnumerable<OptionEntity>> GetByIdAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return GetQueryable(mode)
                .Where(o => ids.Contains(o.Id))
                .ToListAsync(cancellationToken)
                .ContinueWith<IEnumerable<OptionEntity>>(t => t.Result, cancellationToken);
        }
    }
}
