using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoiceById
{
    public sealed class GetPurchaseInvoiceByIdQueryHandler(IPurchaseInvoiceReadRepository repository) : IRequestHandler<GetPurchaseInvoiceByIdQuery, Result<PurchaseInvoiceDetailResponse?>>
    {
        public async Task<Result<PurchaseInvoiceDetailResponse?>> Handle(
            GetPurchaseInvoiceByIdQuery request,
            CancellationToken cancellationToken)
        {
            var invoice = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (invoice is null)
            {
                return Error.NotFound($"Không tìm thấy hóa đơn mua hàng có ID {request.Id}.", "Id");
            }
            return invoice.Adapt<PurchaseInvoiceDetailResponse?>();
        }
    }
}
