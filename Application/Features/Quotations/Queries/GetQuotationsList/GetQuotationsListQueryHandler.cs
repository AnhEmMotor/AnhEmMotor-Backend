using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Quotation;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Quotations.Queries.GetQuotationsList
{
    public sealed class GetQuotationsListQueryHandler(IQuotationReadRepository readRepository) : IRequestHandler<GetQuotationsListQuery, Result<PagedResult<QuotationSummaryResponse?>>>
    {
        public async Task<Result<PagedResult<QuotationSummaryResponse?>>> Handle(
            GetQuotationsListQuery request,
            CancellationToken cancellationToken)
        {
            var sieveModel = request.SieveModel ?? new SieveModel();
            var result = await readRepository.GetPagedAsync<QuotationSummaryResponse?>(
                sieveModel,
                DataFetchMode.ActiveOnly,
                cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
    }
}
