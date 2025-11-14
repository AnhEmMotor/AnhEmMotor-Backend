using Application.ApiContracts.Setting;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Services.Setting;
using Domain.Helpers;

namespace Application.Services.Setting
{
    public class SettingService(ISettingRepository settingRepository) : ISettingService
    {
        public async Task<ErrorResponse?> SetSettingsAsync(List<SetSettingItemRequest> requests)
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
