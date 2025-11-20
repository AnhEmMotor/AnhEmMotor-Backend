using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Services.Setting;
using Domain.Helpers;

namespace Application.Services.Setting;

public class SettingService(
    ISettingRepository settingRepository,
    IUnitOfWork unitOfWork) : ISettingService
{
    public async Task<Dictionary<string, long?>> GetAllSettingsAsync(CancellationToken cancellationToken)
    {
        var settingsList = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        if (settingsList == null || !settingsList.Any())
        {
            return [];
        }

        return settingsList
            .Where(s => s.Key != null)
            .ToDictionary(s => s.Key!, s => s.Value);
    }

    public async Task<(Dictionary<string, long?>? Data, ErrorResponse? Error)> SetSettingsAsync(Dictionary<string, long?> requests, CancellationToken cancellationToken)
    {
        var settingsToUpsert = requests.Select(req => new Domain.Entities.Setting
        {
            Key = req.Key,
            Value = req.Value
        });

        await settingRepository.UpsertBatchAsync(settingsToUpsert, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (requests, null);
    }
}