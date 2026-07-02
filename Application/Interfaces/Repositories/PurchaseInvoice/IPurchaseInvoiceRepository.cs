using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;
using Sieve.Models;

namespace Application.Interfaces.Repositories.PurchaseInvoice
{
    public interface IPurchaseInvoiceReadRepository
    {
        Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default);

        Task<PurchaseInvoiceEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        Task<PurchaseInvoiceEntity?> GetByIdWithItemsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);
    }

    public interface IPurchaseInvoiceInsertRepository
    {
        void Add(PurchaseInvoiceEntity purchaseInvoice);
    }

    public interface IPurchaseInvoiceUpdateRepository
    {
        void Update(PurchaseInvoiceEntity purchaseInvoice);
    }

    public interface IPurchaseInvoiceDeleteRepository
    {
        void Delete(PurchaseInvoiceEntity purchaseInvoice);
    }
}

