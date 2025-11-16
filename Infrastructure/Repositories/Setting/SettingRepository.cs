using Application.Interfaces.Repositories.Setting;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using SettingEntity = Domain.Entities.Setting;

namespace Infrastructure.Repositories.Setting
{
    public class SettingRepository(ApplicationDBContext context) : ISettingRepository
    {
        public Task<IEnumerable<SettingEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return context.Settings.AsNoTracking().ToListAsync(cancellationToken).ContinueWith(
                t => t.Result as IEnumerable<SettingEntity>,
                cancellationToken,
                TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
            );
        }

        public async Task UpsertBatchAsync(IEnumerable<SettingEntity> settings, CancellationToken cancellationToken)
        {
            foreach (var setting in settings)
            {
                var existingSetting = await context.Settings.FirstOrDefaultAsync(s => string.Compare(s.Key, setting.Key) == 0, cancellationToken).ConfigureAwait(false);
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
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
