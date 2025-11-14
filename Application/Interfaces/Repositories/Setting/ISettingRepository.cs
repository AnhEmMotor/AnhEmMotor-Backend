namespace Application.Interfaces.Repositories.Setting
{
    public interface ISettingRepository
    {
        Task UpsertBatchAsync(IEnumerable<Domain.Entities.Setting> settings);
    }
}
