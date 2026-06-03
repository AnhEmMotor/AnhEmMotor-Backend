using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Services;
using Domain.Entities;
using Mapster;
using MediatR;
using Application.Interfaces.Repositories.PurchaseOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice
{
    public sealed class CreatePurchaseInvoiceCommandHandler(
        IPurchaseInvoiceInsertRepository insertRepository,
        IPurchaseInvoiceReadRepository readRepository,
        IVehicleReadRepository vehicleReadRepository,
        IPurchaseOrderReadRepository poReadRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<CreatePurchaseInvoiceCommand, Result<PurchaseInvoiceDetailResponse?>>
    {
        public async Task<Result<PurchaseInvoiceDetailResponse?>> Handle(
            CreatePurchaseInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return Error.BadRequest("Danh sách sản phẩm trên hóa đơn không được trống.", "Items");
            }

            if (request.PurchaseOrderId.HasValue)
            {
                var po = await poReadRepository.GetByIdWithDetailsAsync(request.PurchaseOrderId.Value, cancellationToken).ConfigureAwait(false);
                if (po is null)
                {
                    return Error.NotFound($"Không tìm thấy đơn mua hàng PO {request.PurchaseOrderId.Value}.", "PurchaseOrderId");
                }

                var poItemsDict = po.PurchaseOrderItems.ToDictionary(x => x.Id);
                foreach (var item in request.Items)
                {
                    if (item.PurchaseOrderItemId.HasValue && poItemsDict.TryGetValue(item.PurchaseOrderItemId.Value, out var poItem))
                    {
                        var occupiedQty = poItem.PurchaseInvoiceItems
                            .Where(pii => pii.DeletedAt == null &&
                                          pii.PurchaseInvoice != null &&
                                          pii.PurchaseInvoice.DeletedAt == null &&
                                          (string.Equals(pii.PurchaseInvoice.Status, "approved", StringComparison.OrdinalIgnoreCase) ||
                                           string.Equals(pii.PurchaseInvoice.Status, "draft", StringComparison.OrdinalIgnoreCase)))
                            .Sum(pii => pii.InvoicedQuantity);

                        var remainingAllowed = poItem.OrderedQuantity - occupiedQty;
                        if (item.InvoicedQuantity > remainingAllowed)
                        {
                            var productName = poItem.ProductVariant?.Product?.Name ?? $"Biến thể #{poItem.ProductVariantId}";
                            return Error.BadRequest(
                                $"Số lượng hóa đơn ({item.InvoicedQuantity}) cho sản phẩm '{productName}' vượt quá số lượng còn lại được phép xuất hóa đơn từ đơn mua hàng PO ({remainingAllowed}).",
                                "Items");
                        }
                    }
                }
            }

            var currentUserId = currentUserContext.GetUserId();
            var invoiceItems = new List<PurchaseInvoiceItem>();

            foreach (var item in request.Items)
            {
                var invoiceItem = new PurchaseInvoiceItem
                {
                    PurchaseOrderItemId = item.PurchaseOrderItemId,
                    InventoryReceiptItemId = item.InventoryReceiptItemId,
                    ProductVariantId = item.ProductVariantId,
                    ProductVariantColorId = item.ProductVariantColorId,
                    InvoicedQuantity = item.InvoicedQuantity,
                    UnitPrice = item.UnitPrice,
                    TaxRate = item.TaxRate,
                    Vehicles = new List<Vehicle>()
                };

                // Flow A: Nhập kho trước -> Hóa đơn sau
                if (item.InventoryReceiptItemId.HasValue)
                {
                    var vehicles = await vehicleReadRepository.GetVehiclesByReceiptInfoIdAsync(item.InventoryReceiptItemId.Value, cancellationToken).ConfigureAwait(false);
                    foreach (var v in vehicles)
                    {
                        invoiceItem.Vehicles.Add(v);
                    }
                }
                // Flow B: Hóa đơn trước -> Nhập kho sau (nếu đã biết trước mã VIN trên hóa đơn)
                else if (item.Vehicles != null && item.Vehicles.Count > 0)
                {
                    foreach (var v in item.Vehicles)
                    {
                        invoiceItem.Vehicles.Add(new Vehicle
                        {
                            VinNumber = v.VinNumber.Trim(),
                            EngineNumber = v.EngineNumber.Trim(),
                            LicensePlate = string.Empty,
                            ProductVariantId = item.ProductVariantId,
                            ProductVariantColorId = item.ProductVariantColorId,
                            LeadId = null,
                            PurchaseDate = DateTimeOffset.UtcNow,
                            IsActive = true,
                            Status = Domain.Constants.Order.VehicleStatus.PendingImport,
                            ImportPrice = v.ImportPrice
                        });
                    }
                }

                invoiceItems.Add(invoiceItem);
            }

            var purchaseInvoice = new PurchaseInvoiceEntity
            {
                PurchaseOrderId = request.PurchaseOrderId,
                InvoiceNumber = request.InvoiceNumber,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                Status = "draft",
                Note = request.Note,
                CreatedBy = currentUserId,
                PurchaseInvoiceItems = invoiceItems
            };

            insertRepository.Add(purchaseInvoice);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var created = await readRepository.GetByIdWithDetailsAsync(purchaseInvoice.Id, cancellationToken)
                .ConfigureAwait(false);

            return created.Adapt<PurchaseInvoiceDetailResponse?>();
        }
    }
}
