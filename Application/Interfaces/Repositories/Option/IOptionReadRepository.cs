using Domain.Enums;
using System;
using OptionEntity = Domain.Entities.Option;

namespace Application.Interfaces.Repositories.Option
{
    public interface IOptionReadRepository
    {
        IQueryable<OptionEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

        Task<IEnumerable<OptionEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        Task<OptionEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        Task<IEnumerable<OptionEntity>> GetByIdAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);
    }
}
