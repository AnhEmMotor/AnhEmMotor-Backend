using Application.Interfaces.Repositories.PredefinedOption;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.PredefinedOption;

public class PredefinedOptionReadRepository(ApplicationDBContext context) : IPredefinedOptionReadRepository
{
    public Task<Dictionary<string, string>> GetAllAsDictionaryAsync(CancellationToken cancellationToken)
    {
        return context.PredefinedOptions
            .OrderBy(p => p.Key)
            .ToDictionaryAsync(p => p.Key, p => p.Value, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetAllKeysAsync(CancellationToken cancellationToken)
    {
        var result = await context.PredefinedOptions
            .Select(p => p.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return result;
    }
}