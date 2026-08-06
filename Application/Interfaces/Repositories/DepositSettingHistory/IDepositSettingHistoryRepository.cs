
namespace Application.Interfaces.Repositories.DepositSettingHistory
{
    public interface IDepositSettingHistoryRepository
    {
        public Task<List<Domain.Entities.DepositSettingHistory>> GetHistoryAsync(CancellationToken cancellationToken);

        public void Add(Domain.Entities.DepositSettingHistory entity);

        public void AddRange(IEnumerable<Domain.Entities.DepositSettingHistory> entities);
    }
}
