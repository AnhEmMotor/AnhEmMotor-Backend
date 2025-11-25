using Application;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Option;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using OptionEntity = Domain.Entities.Option;

namespace Infrastructure.Repositories.Option
{
    public class OptionReadRepository(ApplicationDBContext context) : IOptionReadRepository
    {
        public IQueryable<OptionEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<OptionEntity>(mode);
        }

        public async Task<IEnumerable<OptionEntity>> GetAllAsync(CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return await GetQueryable(mode)
                .ToListAsync(cancellationToken);
        }

        public async Task<OptionEntity?> GetByIdAsync(int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return await GetQueryable(mode)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<OptionEntity>> GetByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return await GetQueryable(mode)
                .Where(o => ids.Contains(o.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
