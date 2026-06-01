using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Sieve.Models;
using System.Threading;
using System.Threading.Tasks;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Application.Interfaces.Repositories.PurchaseInvoice
{
    public interface IPurchaseInvoiceReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default);

        public Task<PurchaseInvoiceEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<PurchaseInvoiceEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);
    }
}
