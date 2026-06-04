using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PurchaseOrder;
using Domain.Constants;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;
using System.Collections.Generic;

namespace Application.Features.PurchaseOrders.Queries.GetApprovedPurchaseOrderForInputById
{
    public sealed class GetApprovedPurchaseOrderForInputByIdQueryHandler(IPurchaseOrderReadRepository repository) 
        : IRequestHandler<GetApprovedPurchaseOrderForInputByIdQuery, Result<PurchaseOrderDetailForInputResponse?>>
    {
        public async Task<Result<PurchaseOrderDetailForInputResponse?>> Handle(
            GetApprovedPurchaseOrderForInputByIdQuery request,
            CancellationToken cancellationToken)
        {
            var po = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (po is null || po.Status != PurchaseOrderStatus.Approved)
            {
                return Error.NotFound($"Không tìm thấy đơn chốt mua đã phê duyệt có ID {request.Id}.", "Id");
            }
            
            var response = po.Adapt<PurchaseOrderDetailForInputResponse?>();

            if (response != null && request.ExcludeReceiptId.HasValue)
            {
                foreach (var responseItem in response.Items)
                {
                    var poItem = po.PurchaseOrderItems.FirstOrDefault(x => x.Id == responseItem.Id);
                    if (poItem != null)
                    {
                        var excludedInfos = poItem.InventoryReceiptInfos
                            .Where(ii => ii.InventoryReceiptId == request.ExcludeReceiptId.Value && ii.DeletedAt == null);

                        var excludedImported = excludedInfos
                            .Where(ii => ii.InventoryReceipt != null &&
                                         string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, System.StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(ii => ii.Count ?? 0);

                        var excludedSent = excludedInfos
                            .Where(ii => ii.InventoryReceipt != null &&
                                         string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Sent, System.StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(ii => ii.Count ?? 0);

                        var excludedDraft = excludedInfos
                            .Where(ii => ii.InventoryReceipt != null &&
                                         string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Draft, System.StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(ii => ii.Count ?? 0);

                        responseItem.ImportedQuantity -= excludedImported;
                        responseItem.SentQuantity -= excludedSent;
                        responseItem.DraftQuantity -= excludedDraft;
                        responseItem.RemainingQuantity += (excludedImported + excludedSent + excludedDraft);

                        var currentReceiptVins = excludedInfos
                            .SelectMany(ii => ii.Vehicles.Where(v => v.DeletedAt == null))
                            .Select(v => v.VinNumber.Trim().ToLowerInvariant())
                            .ToHashSet();

                        var poItemInvoiceVehicles = poItem.PurchaseInvoiceItems
                            .Where(pii => pii.DeletedAt == null && pii.PurchaseInvoice != null && pii.PurchaseInvoice.DeletedAt == null)
                            .SelectMany(pii => pii.Vehicles.Where(v => v.DeletedAt == null))
                            .Where(v => currentReceiptVins.Contains(v.VinNumber.Trim().ToLowerInvariant()))
                            .ToList();

                        var mappedInvoiceVehicles = poItemInvoiceVehicles.Adapt<List<PurchaseOrderVehicleInputResponse>>();
                        foreach (var mv in mappedInvoiceVehicles)
                        {
                            if (!responseItem.InvoicedVehicles.Any(v => string.Equals(v.VinNumber.Trim(), mv.VinNumber.Trim(), System.StringComparison.OrdinalIgnoreCase)))
                            {
                                responseItem.InvoicedVehicles.Add(mv);
                            }
                        }
                    }
                }
            }

            return response;
        }
    }
}
