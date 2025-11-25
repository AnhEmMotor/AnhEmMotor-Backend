using Application.Interfaces.Repositories.Setting;
using MediatR;

namespace Application.Features.Settings.Queries.GetAllSettings;

public sealed class GetAllSettingsQueryHandler(ISettingRepository settingRepository) : IRequestHandler<GetAllSettingsQuery, Dictionary<string, long?>>
{
    public async Task<Dictionary<string, long?>> Handle(
        GetAllSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settingsList = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        if(settingsList == null || !settingsList.Any())
        {
            return [];
        }

        return settingsList
            .Where(s => s.Key != null)
            .ToDictionary(s => s.Key!, s => s.Value);
    }
}
