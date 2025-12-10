using Domain.Constants;
using System;
using OptionEntity = Domain.Entities.Option;

namespace Application.Interfaces.Repositories.Option
{
    public interface IOptionReadRepository
    {
        public IQueryable<OptionEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<IEnumerable<OptionEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<OptionEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<IEnumerable<OptionEntity>> GetByIdAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);
    }
}
