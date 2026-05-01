using Application.Common.Models;
using Application.Interfaces.Repositories.Setting;
using Domain.Constants;
using MediatR;

namespace Application.Features.Settings.Queries.GetStoreSettings;

public class GetStoreSettingsQueryHandler(ISettingRepository settingRepository)
    : IRequestHandler<GetStoreSettingsQuery, Result<Dictionary<string, string?>>>
{
    public async Task<Result<Dictionary<string, string?>>> Handle(
        GetStoreSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var publicKeys = new[] { SettingKeys.OrderValueExceeds, SettingKeys.DepositRatio };
        
        var filtered = settings
            .Where(s => publicKeys.Contains(s.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(s => s.Key, s => s.Value);

        return filtered;
    }
}
