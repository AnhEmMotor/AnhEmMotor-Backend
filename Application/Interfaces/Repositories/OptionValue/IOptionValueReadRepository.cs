using Domain.Enums;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Interfaces.Repositories.OptionValue
{
    public interface IOptionValueReadRepository
    {
        IQueryable<OptionValueEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);
        Task<List<OptionValueEntity>> GetByIdAsync(List<int> optionValueIds, CancellationToken cancellationToken);
        Task<OptionValueEntity?> GetByIdAndNameAsync(int optionId, string name, CancellationToken cancellationToken);
        Task<List<OptionValueEntity>> GetByIdAndNameAsync(List<int> optionIds, List<string> names, CancellationToken cancellationToken);
    }
}
