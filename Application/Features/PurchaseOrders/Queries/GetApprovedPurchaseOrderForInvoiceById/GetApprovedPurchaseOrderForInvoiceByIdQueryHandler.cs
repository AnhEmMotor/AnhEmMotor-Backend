using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseOrder;
using Domain.Constants;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PurchaseOrders.Queries.GetApprovedPurchaseOrderForInvoiceById
{
    public sealed class GetApprovedPurchaseOrderForInvoiceByIdQueryHandler(IPurchaseOrderReadRepository repository) 
        : IRequestHandler<GetApprovedPurchaseOrderForInvoiceByIdQuery, Result<PurchaseOrderDetailForInvoiceResponse?>>
    {
        public async Task<Result<PurchaseOrderDetailForInvoiceResponse?>> Handle(
            GetApprovedPurchaseOrderForInvoiceByIdQuery request,
            CancellationToken cancellationToken)
        {
            var po = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (po is null || po.Status != PurchaseOrderStatus.Approved)
            {
                return Error.NotFound($"Không tìm thấy đơn chốt mua đã phê duyệt có ID {request.Id}.", "Id");
            }

            var response = po.Adapt<PurchaseOrderDetailForInvoiceResponse?>();

            if (response != null && request.ExcludeInvoiceId.HasValue)
            {
                foreach (var responseItem in response.Items)
                {
                    var poItem = po.PurchaseOrderItems.FirstOrDefault(x => x.Id == responseItem.Id);
                    if (poItem != null)
                    {
                        var excludedItems = poItem.PurchaseInvoiceItems
                            .Where(pii => pii.PurchaseInvoiceId == request.ExcludeInvoiceId.Value && pii.DeletedAt == null);

                        var excludedInvoiced = excludedItems
                            .Where(pii => pii.PurchaseInvoice != null &&
                                          string.Compare(pii.PurchaseInvoice.Status, "approved", StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(pii => pii.InvoicedQuantity);

                        var excludedInvoicing = excludedItems
                            .Where(pii => pii.PurchaseInvoice != null &&
                                          string.Compare(pii.PurchaseInvoice.Status, "draft", StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(pii => pii.InvoicedQuantity);

                        responseItem.InvoicedQuantity -= excludedInvoiced;
                        responseItem.InvoicingQuantity -= excludedInvoicing;
                        responseItem.RemainingQuantity += (excludedInvoiced + excludedInvoicing);
                    }
                }
            }

            return response;
        }
    }
}
