using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
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

        public Task<PurchaseInvoiceEntity?> GetByIdWithItemsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);
    }

    public interface IPurchaseInvoiceInsertRepository
    {
        public void Add(PurchaseInvoiceEntity purchaseInvoice);
    }

    public interface IPurchaseInvoiceUpdateRepository
    {
        public void Update(PurchaseInvoiceEntity purchaseInvoice);
    }

    public interface IPurchaseInvoiceDeleteRepository
    {
        public void Delete(PurchaseInvoiceEntity purchaseInvoice);
    }
}

