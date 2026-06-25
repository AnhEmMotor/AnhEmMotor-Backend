using Application.ApiContracts.Client.Invoices;
using MediatR;
using System;

namespace Application.Features.Client.Invoices
{
    public record GetMyInvoicesQuery() : IRequest<List<InvoiceSummaryResponse>>;

    public record GetInvoiceDetailQuery(int Id) : IRequest<InvoiceDetailResponse?>;

    public class GetMyInvoicesHandler : IRequestHandler<GetMyInvoicesQuery, List<InvoiceSummaryResponse>>
    {
        public async Task<List<InvoiceSummaryResponse>> Handle(
            GetMyInvoicesQuery request,
            CancellationToken cancellationToken)
        {
            return await Task.FromResult(
                new List<InvoiceSummaryResponse>
                {
                    new InvoiceSummaryResponse(1, "INV-001", DateTime.UtcNow, 50000000, "Vehicle Purchase"),
                    new InvoiceSummaryResponse(2, "INV-002", DateTime.UtcNow.AddDays(-10), 1500000, "Service & Parts")
                });
        }
    }

    public class GetInvoiceDetailHandler : IRequestHandler<GetInvoiceDetailQuery, InvoiceDetailResponse?>
    {
        public async Task<InvoiceDetailResponse?> Handle(
            GetInvoiceDetailQuery request,
            CancellationToken cancellationToken)
        {
            return await Task.FromResult(
                new InvoiceDetailResponse(
                    request.Id,
                    "INV-001",
                    DateTime.UtcNow,
                    50000000,
                    new List<InvoiceItemDto> { new InvoiceItemDto("Honda SH 150i", 1, 50000000, 50000000) },
                    "https://storage.anhmmotor.vn/invoices/inv001.pdf"));
        }
    }
}
