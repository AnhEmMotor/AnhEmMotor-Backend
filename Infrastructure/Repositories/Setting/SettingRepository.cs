using Application.Interfaces.Repositories.Setting;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using SettingEntity = Domain.Entities.Setting;

namespace Infrastructure.Repositories.Setting;

public class SettingRepository(ApplicationDBContext context) : ISettingRepository
{
    public Task<IEnumerable<SettingEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.Settings
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<SettingEntity>>(t => t.Result, cancellationToken);
    }

    public void Update(IEnumerable<SettingEntity> settings)
    {
        var settingKeys = settings.Select(s => s.Key).ToList();

        var existingSettings = context.Settings.Where(s => settingKeys.Contains(s.Key)).ToList();

        foreach(var setting in settings)
        {
            var existingSetting = existingSettings.FirstOrDefault(s => string.Compare(s.Key, setting.Key) == 0);

            if(existingSetting != null)
            {
                existingSetting.Value = setting.Value;
                context.Settings.Update(existingSetting);
            } else
            {
                context.Settings.Add(setting);
            }
        }
    }
}