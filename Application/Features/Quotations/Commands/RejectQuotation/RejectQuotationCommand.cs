using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Quotations.Commands.RejectQuotation
{
    public sealed record RejectQuotationCommand(int Id) : IRequest<Result<QuotationDetailResponse?>>;
}
