using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Quotations.Commands.SendQuotation
{
    public sealed record SendQuotationCommand(int Id) : IRequest<Result<QuotationDetailResponse?>>;
}
