using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Sieve.Models;
using QuotationEntity = Domain.Entities.Quotation;

namespace Application.Interfaces.Repositories.Quotation
{
    public interface IQuotationReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default);

        public Task<QuotationEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<QuotationEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<List<QuotationProductRow>> GetApprovedQuotationRowsByVariantsAsync(
            IEnumerable<int> variantIds,
            CancellationToken cancellationToken);

        public Task<List<QuotationProductRow>> GetRowsByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken);
    }
}
