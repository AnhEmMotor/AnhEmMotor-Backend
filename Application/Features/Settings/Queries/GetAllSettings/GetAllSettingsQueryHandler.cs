using Application.Common.Models;
using Application.Interfaces.Repositories.Setting;
using MediatR;

namespace Application.Features.Settings.Queries.GetAllSettings;

public sealed class GetAllSettingsQueryHandler(ISettingRepository settingRepository) : IRequestHandler<GetAllSettingsQuery, Result<Dictionary<string, string?>>>
{
    public async Task<Result<Dictionary<string, string?>>> Handle(
        GetAllSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settingsList = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        if(settingsList == null || !settingsList.Any())
        {
            return Error.BadRequest("Không có thông số cài đặt nào trong hệ thống (hệ thống chắc chắn sẽ gặp lỗi!)");
        }

        return settingsList
            .Where(s => s.Key != null)
            .ToDictionary(s => s.Key!, s => s.Value);
    }
}
