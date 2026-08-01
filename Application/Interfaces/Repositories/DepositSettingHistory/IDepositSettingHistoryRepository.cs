using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.DepositSettingHistory
{
    public interface IDepositSettingHistoryRepository
    {
        Task<List<Domain.Entities.DepositSettingHistory>> GetHistoryAsync(CancellationToken cancellationToken);
        void Add(Domain.Entities.DepositSettingHistory entity);
        void AddRange(IEnumerable<Domain.Entities.DepositSettingHistory> entities);
    }
}
