using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Services.Setting;
using Domain.Helpers;

namespace Application.Services.Setting
{
    public class SettingService(ISettingRepository settingRepository) : ISettingService
    {
        public async Task<Dictionary<string, long?>> GetAllSettingsAsync()
        {
            var settingsList = await settingRepository.GetAllAsync();

            if (settingsList == null || !settingsList.Any())
            {
                return [];
            }

            var settingsDictionary = settingsList
                .Where(s => s.Key != null)
                .ToDictionary(s => s.Key!, s => s.Value);

            return settingsDictionary;
        }

        public async Task<ErrorResponse?> SetSettingsAsync(Dictionary<string, long?> requests)
        {
            try
            {
                var settingsToUpsert = requests.Select(req => new Domain.Entities.Setting
                {
                    Key = req.Key,
                    Value = req.Value
                });

                await settingRepository.UpsertBatchAsync(settingsToUpsert);

                return null;
            }
            catch (Exception ex)
            {
                return new ErrorResponse
                {
                    Errors =
                    [
                        new ErrorDetail { Message = ex.Message }
                    ]
                };
            }
        }
    }
}
