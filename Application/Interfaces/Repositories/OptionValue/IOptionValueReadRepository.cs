using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Interfaces.Repositories.OptionValue
{
    public interface IOptionValueReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<OptionValueEntity, bool>>? filter = null,
            CancellationToken cancellationToken = default);

        public Task<List<OptionValueEntity>> GetByIdAsync(List<int> optionValueIds, CancellationToken cancellationToken);

        public Task<OptionValueEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);

        public Task<OptionValueEntity?> GetByIdAndNameAsync(
            int optionId,
            string name,
            CancellationToken cancellationToken);

        public Task<List<OptionValueEntity>> GetByIdAndNameAsync(
            List<int> optionIds,
            List<string> names,
            CancellationToken cancellationToken);
    }
}
