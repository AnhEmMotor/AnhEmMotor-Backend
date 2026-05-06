using Domain.Constants;
using TechnologyEntity = Domain.Entities.Technology;

namespace Application.Interfaces.Repositories.Technology
{
    public interface ITechnologyReadRepository
    {
        public IQueryable<TechnologyEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<IEnumerable<TechnologyEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<TechnologyEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<IEnumerable<TechnologyEntity>> GetAllWithCategoryAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);
    }
}
