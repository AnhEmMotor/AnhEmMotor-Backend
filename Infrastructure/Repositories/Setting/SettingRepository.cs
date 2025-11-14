using Application.Interfaces.Repositories.Setting;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Setting
{
    public class SettingRepository(ApplicationDBContext context) : ISettingRepository
    {
        public async Task UpsertBatchAsync(IEnumerable<Domain.Entities.Setting> settings)
        {
            foreach (var setting in settings)
            {
                var existingSetting = await context.Settings.FirstOrDefaultAsync(s => s.Key == setting.Key);
                if (existingSetting != null)
                {
                    existingSetting.Value = setting.Value;
                    context.Settings.Update(existingSetting);
                }
                else
                {
                    context.Settings.Add(setting);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
