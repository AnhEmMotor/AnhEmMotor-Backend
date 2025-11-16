using Domain.Helpers;

namespace Application.Interfaces.Services.Setting
{
    public interface ISettingService
    {
        Task<Dictionary<string, long?>> GetAllSettingsAsync(CancellationToken cancellationToken);
        Task<ErrorResponse?> SetSettingsAsync(Dictionary<string, long?> requests, CancellationToken cancellationToken);
    }
}
