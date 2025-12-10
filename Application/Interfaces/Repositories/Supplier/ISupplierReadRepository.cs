using Application.ApiContracts.Supplier.Responses;
using Domain.Constants;
using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierReadRepository
    {
        public IQueryable<SupplierEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

        public IQueryable<SupplierWithTotalInputResponse> GetQueryableWithTotalInput(
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<IEnumerable<SupplierEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<SupplierEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<IEnumerable<SupplierEntity>> GetByIdAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);
    }
}
