using SettingEntity = Domain.Entities.Setting;

namespace Application.Interfaces.Repositories.Setting
{
    public interface ISettingRepository
    {
        Task<IEnumerable<SettingEntity>> GetAllAsync(CancellationToken cancellationToken);
        Task UpsertBatchAsync(IEnumerable<SettingEntity> settings, CancellationToken cancellationToken);
    }
}
