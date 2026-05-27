using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Quotation;
using Domain.Constants;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Quotations.Queries.GetQuotationById
{
    public sealed class GetQuotationByIdQueryHandler(
        IQuotationReadRepository readRepository) : IRequestHandler<GetQuotationByIdQuery, Result<QuotationDetailResponse?>>
    {
        public async Task<Result<QuotationDetailResponse?>> Handle(
            GetQuotationByIdQuery request,
            CancellationToken cancellationToken)
        {
            var quotation = await readRepository.GetByIdWithDetailsAsync(
                request.Id,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);

            if (quotation is null)
            {
                return Error.NotFound($"Yêu cầu báo giá {request.Id} không tồn tại hoặc đã bị xóa.", "Id");
            }

            return quotation.Adapt<QuotationDetailResponse>();
        }
    }
}
