using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Mapster;
using MediatR;

namespace Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoiceById
{
    public class GetPurchaseInvoiceByIdQueryHandler(IPurchaseInvoiceReadRepository repository)
        : IRequestHandler<GetPurchaseInvoiceByIdQuery, Result<PurchaseInvoiceDetailResponse?>>
    {
        public async Task<Result<PurchaseInvoiceDetailResponse?>> Handle(
            GetPurchaseInvoiceByIdQuery request,
            CancellationToken cancellationToken)
        {
            var pi = await repository.GetByIdWithItemsAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (pi is null)
                return Error.NotFound($"Khong tim thay hoa don mua hang co ID {request.Id}.", "Id");
            return pi.Adapt<PurchaseInvoiceDetailResponse?>();
        }
    }
}
