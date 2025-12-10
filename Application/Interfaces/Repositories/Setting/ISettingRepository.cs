using SettingEntity = Domain.Entities.Setting;

namespace Application.Interfaces.Repositories.Setting
{
    public interface ISettingRepository
    {
        public Task<IEnumerable<SettingEntity>> GetAllAsync(CancellationToken cancellationToken);

        public void Update(IEnumerable<SettingEntity> settings);
    }
}
