using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Quotations.Commands.ApproveQuotation
{
    public sealed record ApproveQuotationCommand(int Id) : IRequest<Result<QuotationDetailResponse?>>;
}
