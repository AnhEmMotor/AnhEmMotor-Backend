using Application.ApiContracts.Client.Invoices;
using Application.Interfaces.Repositories.Invoice;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Client.Invoices
{
    public record GetMyInvoicesQuery(Guid UserId) : IRequest<List<InvoiceSummaryResponse>>;

    public record GetInvoiceDetailQuery(int Id) : IRequest<InvoiceDetailResponse?>;

    public class GetMyInvoicesHandler : IRequestHandler<GetMyInvoicesQuery, List<InvoiceSummaryResponse>>
    {
        private readonly IInvoiceReadRepository _repository;

        public GetMyInvoicesHandler(IInvoiceReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<InvoiceSummaryResponse>> Handle(GetMyInvoicesQuery request, CancellationToken cancellationToken)
        {
            var invoices = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
            return invoices.Select(inv => new InvoiceSummaryResponse(
                inv.Id,
                inv.InvoiceNumber,
                inv.IssueDate,
                inv.TotalAmount,
                inv.Type
            )).ToList();
        }
    }

    public class GetInvoiceDetailHandler : IRequestHandler<GetInvoiceDetailQuery, InvoiceDetailResponse?>
    {
        private readonly IInvoiceReadRepository _repository;

        public GetInvoiceDetailHandler(IInvoiceReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<InvoiceDetailResponse?> Handle(GetInvoiceDetailQuery request, CancellationToken cancellationToken)
        {
            var invoice = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (invoice == null) return null;

            return new InvoiceDetailResponse(
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.IssueDate,
                invoice.TotalAmount,
                new List<InvoiceItemDto>
                {
                    new InvoiceItemDto(invoice.Type, 1, invoice.TotalAmount, invoice.TotalAmount)
                },
                $"https://storage.anhmmotor.vn/invoices/{invoice.InvoiceNumber.ToLower()}.pdf"
            );
        }
    }
}
