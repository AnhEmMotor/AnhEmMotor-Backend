using SettingEntity = Domain.Entities.Setting;

namespace Application.Interfaces.Repositories.Setting
{
    public interface ISettingRepository
    {
        Task<IEnumerable<SettingEntity>> GetAllAsync();
        Task UpsertBatchAsync(IEnumerable<SettingEntity> settings);
    }
}
