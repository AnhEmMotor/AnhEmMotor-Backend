using Domain.Constants;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Interfaces.Repositories.OptionValue
{
    public interface IOptionValueReadRepository
    {
        public IQueryable<OptionValueEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<List<OptionValueEntity>> GetByIdAsync(List<int> optionValueIds, CancellationToken cancellationToken);

        public Task<OptionValueEntity?> GetByIdAndNameAsync(int optionId, string name, CancellationToken cancellationToken);

        public Task<List<OptionValueEntity>> GetByIdAndNameAsync(
            List<int> optionIds,
            List<string> names,
            CancellationToken cancellationToken);
    }
}
