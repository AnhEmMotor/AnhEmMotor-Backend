using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        public Task<List<Domain.Entities.QuotationProductRow>> GetApprovedQuotationRowsByVariantsAsync(
            IEnumerable<int> variantIds,
            CancellationToken cancellationToken);

        public Task<List<Domain.Entities.QuotationProductRow>> GetRowsByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken);
    }
}
