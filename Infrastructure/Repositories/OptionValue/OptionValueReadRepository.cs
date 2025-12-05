using Application.Interfaces.Repositories.OptionValue;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Infrastructure.Repositories.OptionValue
{
    public class OptionValueReadRepository(ApplicationDBContext context) : IOptionValueReadRepository
    {
        public Task<List<OptionValueEntity>> GetByIdAsync(List<int> optionValueIds, CancellationToken cancellationToken)
        {
            return context.OptionValues
                .Include(ov => ov.Option)
                .Where(ov => optionValueIds.Contains(ov.Id))
                .ToListAsync(cancellationToken);
        }

        public Task<OptionValueEntity?> GetByIdAndNameAsync(
            int optionId,
            string name,
            CancellationToken cancellationToken)
        {
            return context.OptionValues
                .FirstOrDefaultAsync(
                    ov => ov.OptionId == optionId && string.Compare(ov.Name, name) == 0,
                    cancellationToken);
        }

        public Task<List<OptionValueEntity>> GetByIdAndNameAsync(
            List<int> optionIds,
            List<string> names,
            CancellationToken cancellationToken)
        {
            return GetQueryable()
                .Where(
                    ov => ov.OptionId.HasValue &&
                        ov.Name != null &&
                        optionIds.Contains(ov.OptionId.Value) &&
                        names.Contains(ov.Name))
                .ToListAsync(cancellationToken)
                .ContinueWith(t => t.Result, cancellationToken);
            ;
        }

        public IQueryable<OptionValueEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        { return context.GetQuery<OptionValueEntity>(mode); }
    }
}
