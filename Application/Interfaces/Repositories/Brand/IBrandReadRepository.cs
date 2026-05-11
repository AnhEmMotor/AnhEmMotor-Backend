using Domain.Primitives;
using Domain.Constants;
using Sieve.Models;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandReadRepository
    {
        public Task<Domain.Primitives.PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default);

        public Task<IEnumerable<BrandEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<BrandEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<IEnumerable<BrandEntity>> GetByIdAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<List<BrandEntity>> GetByNameAsync(
            string name,
            CancellationToken cancellationToken,
            DataFetchMode dataFetchMode = DataFetchMode.ActiveOnly);
    }
}
