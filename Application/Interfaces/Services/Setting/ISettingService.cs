using Application.ApiContracts.Setting;
using Domain.Helpers;

namespace Application.Interfaces.Services.Setting
{
    public interface ISettingService
    {
        Task<ErrorResponse?> SetSettingsAsync(List<SetSettingItemRequest> requests);
    }
}
