using Domain.Constants;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandReadRepository
    {
        public IQueryable<BrandEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

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

        public Task<IEnumerable<BrandEntity>> GetByNameAsync(
            string name,
            CancellationToken cancellationToken,
            DataFetchMode dataFetchMode = DataFetchMode.ActiveOnly);
    }
}
