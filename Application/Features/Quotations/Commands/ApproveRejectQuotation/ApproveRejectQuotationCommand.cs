using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Quotations.Commands.ApproveRejectQuotation
{
    public sealed record ApproveRejectQuotationCommand(int Id, string Status) : IRequest<Result<QuotationDetailResponse?>>;
}
