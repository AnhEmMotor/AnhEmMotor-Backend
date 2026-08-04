using Application.Interfaces.Repositories.DepositSettingHistory;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.DepositSettingHistory
{
    public class DepositSettingHistoryRepository(ApplicationDBContext context) : IDepositSettingHistoryRepository
    {
        private readonly ApplicationDBContext _context = context;

        public Task<List<Domain.Entities.DepositSettingHistory>> GetHistoryAsync(CancellationToken cancellationToken)
        {
            return _context.DepositSettingHistories
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public void Add(Domain.Entities.DepositSettingHistory entity)
        {
            _context.DepositSettingHistories.Add(entity);
        }

        public void AddRange(IEnumerable<Domain.Entities.DepositSettingHistory> entities)
        {
            _context.DepositSettingHistories.AddRange(entities);
        }
    }
}
