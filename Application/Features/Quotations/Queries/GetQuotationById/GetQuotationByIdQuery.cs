using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Quotations.Queries.GetQuotationById
{
    public sealed record GetQuotationByIdQuery : IRequest<Result<QuotationDetailResponse?>>
    {
        public int Id { get; init; }
    }
}
