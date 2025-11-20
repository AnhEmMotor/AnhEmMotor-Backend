using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierInsertRepository
    {
        Task AddAsync(SupplierEntity supplier, CancellationToken cancellationToken);
    }
}
