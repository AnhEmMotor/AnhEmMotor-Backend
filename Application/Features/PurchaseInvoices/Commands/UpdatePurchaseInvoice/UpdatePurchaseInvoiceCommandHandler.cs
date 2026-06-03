using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Application.Interfaces.Repositories.PurchaseOrder;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PurchaseInvoiceEntity = Domain.Entities.PurchaseInvoice;

namespace Application.Features.PurchaseInvoices.Commands.UpdatePurchaseInvoice
{
    public sealed class UpdatePurchaseInvoiceCommandHandler(
        IPurchaseInvoiceReadRepository readRepository,
        IPurchaseInvoiceUpdateRepository updateRepository,
        IPurchaseInvoiceDeleteRepository deleteRepository,
        IPurchaseOrderReadRepository poReadRepository,
        Application.Interfaces.Repositories.Vehicle.IVehicleUpdateRepository vehicleUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdatePurchaseInvoiceCommand, Result<PurchaseInvoiceDetailResponse?>>
    {
        public async Task<Result<PurchaseInvoiceDetailResponse?>> Handle(
            UpdatePurchaseInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            var invoice = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (invoice is null)
            {
                return Error.NotFound($"Không tìm thấy hóa đơn mua hàng có ID {request.Id}.", "Id");
            }

            if (!string.Equals(invoice.Status, "draft", StringComparison.OrdinalIgnoreCase))
            {
                return Error.BadRequest("Không thể chỉnh sửa hóa đơn mua hàng đã duyệt hoặc gửi.", "Status");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return Error.BadRequest("Danh sách sản phẩm trên hóa đơn không được trống.", "Items");
            }

            var purchaseOrderId = invoice.PurchaseOrderId;
            if (purchaseOrderId.HasValue)
            {
                var po = await poReadRepository.GetByIdWithDetailsAsync(purchaseOrderId.Value, cancellationToken).ConfigureAwait(false);
                if (po is null)
                {
                    return Error.NotFound($"Không tìm thấy đơn mua hàng PO {purchaseOrderId.Value}.", "PurchaseOrderId");
                }

                var poItemsDict = po.PurchaseOrderItems.ToDictionary(x => x.Id);
                foreach (var item in request.Items)
                {
                    var purchaseOrderItemId = item.PurchaseOrderItemId;
                    if (!purchaseOrderItemId.HasValue && item.Id.HasValue && item.Id > 0)
                    {
                        var existingItem = invoice.PurchaseInvoiceItems.FirstOrDefault(x => x.Id == item.Id.Value);
                        if (existingItem != null)
                        {
                            purchaseOrderItemId = existingItem.PurchaseOrderItemId;
                        }
                    }

                    if (purchaseOrderItemId.HasValue && poItemsDict.TryGetValue(purchaseOrderItemId.Value, out var poItem))
                    {
                        var occupiedQty = poItem.PurchaseInvoiceItems
                            .Where(pii => pii.DeletedAt == null &&
                                          pii.PurchaseInvoiceId != request.Id && // exclude current invoice
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

            invoice.InvoiceNumber = request.InvoiceNumber;
            invoice.InvoiceDate = request.InvoiceDate;
            invoice.DueDate = request.DueDate;
            invoice.Note = request.Note;

            var existingItemsDict = invoice.PurchaseInvoiceItems.ToDictionary(x => x.Id);
            var requestItemsDict = request.Items.Where(x => x.Id.HasValue && x.Id > 0).ToDictionary(x => x.Id!.Value);

            // Delete
            var toDelete = invoice.PurchaseInvoiceItems.Where(x => !requestItemsDict.ContainsKey(x.Id)).ToList();
            foreach (var item in toDelete)
            {
                deleteRepository.DeleteItem(item);
                invoice.PurchaseInvoiceItems.Remove(item);
            }

            // Add or Update
            foreach (var itemRequest in request.Items)
            {
                if (itemRequest.Id.HasValue && itemRequest.Id > 0)
                {
                    if (existingItemsDict.TryGetValue(itemRequest.Id.Value, out var existingItem))
                    {
                        existingItem.PurchaseOrderItemId = itemRequest.PurchaseOrderItemId;
                        existingItem.InventoryReceiptItemId = itemRequest.InventoryReceiptItemId;
                        existingItem.ProductVariantId = itemRequest.ProductVariantId;
                        existingItem.ProductVariantColorId = itemRequest.ProductVariantColorId;
                        existingItem.InvoicedQuantity = itemRequest.InvoicedQuantity;
                        existingItem.UnitPrice = itemRequest.UnitPrice;
                        existingItem.TaxRate = itemRequest.TaxRate;

                        if (itemRequest.Vehicles != null)
                        {
                            var reqVehiclesDict = itemRequest.Vehicles
                                .Where(v => v.Id.HasValue && v.Id > 0)
                                .ToDictionary(v => v.Id!.Value);

                            // 1. Delete vehicles that are no longer in the request
                            var vehiclesToRemove = existingItem.Vehicles
                                .Where(v => !reqVehiclesDict.ContainsKey(v.Id) && 
                                            !itemRequest.Vehicles.Any(rv => !rv.Id.HasValue && 
                                                                            string.Equals(rv.VinNumber.Trim(), v.VinNumber.Trim(), StringComparison.OrdinalIgnoreCase) && 
                                                                            string.Equals(rv.EngineNumber.Trim(), v.EngineNumber.Trim(), StringComparison.OrdinalIgnoreCase)))
                                .ToList();

                            foreach (var v in vehiclesToRemove)
                            {
                                existingItem.Vehicles.Remove(v);
                                if (v.InventoryReceiptInfoId == null)
                                {
                                    vehicleUpdateRepository.Remove(v);
                                }
                            }

                            // 2. Add or Update
                            foreach (var vehicleRequest in itemRequest.Vehicles)
                            {
                                Vehicle? existingVehicle = null;
                                if (vehicleRequest.Id.HasValue && vehicleRequest.Id.Value > 0)
                                {
                                    existingVehicle = existingItem.Vehicles.FirstOrDefault(v => v.Id == vehicleRequest.Id.Value);
                                }
                                else
                                {
                                    existingVehicle = existingItem.Vehicles.FirstOrDefault(v => 
                                        string.Equals(v.VinNumber.Trim(), vehicleRequest.VinNumber.Trim(), StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(v.EngineNumber.Trim(), vehicleRequest.EngineNumber.Trim(), StringComparison.OrdinalIgnoreCase));
                                }

                                if (existingVehicle != null)
                                {
                                    existingVehicle.ImportPrice = vehicleRequest.ImportPrice;
                                    if (existingVehicle.InventoryReceiptInfoId == null)
                                    {
                                        existingVehicle.VinNumber = vehicleRequest.VinNumber.Trim();
                                        existingVehicle.EngineNumber = vehicleRequest.EngineNumber.Trim();
                                    }
                                }
                                else
                                {
                                    existingItem.Vehicles.Add(new Vehicle
                                    {
                                        VinNumber = vehicleRequest.VinNumber.Trim(),
                                        EngineNumber = vehicleRequest.EngineNumber.Trim(),
                                        LicensePlate = string.Empty,
                                        ProductVariantId = itemRequest.ProductVariantId,
                                        ProductVariantColorId = itemRequest.ProductVariantColorId,
                                        LeadId = null,
                                        PurchaseDate = DateTimeOffset.UtcNow,
                                        IsActive = true,
                                        Status = Domain.Constants.Order.VehicleStatus.PendingImport,
                                        ImportPrice = vehicleRequest.ImportPrice
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    var newItem = new PurchaseInvoiceItem
                    {
                        PurchaseOrderItemId = itemRequest.PurchaseOrderItemId,
                        InventoryReceiptItemId = itemRequest.InventoryReceiptItemId,
                        ProductVariantId = itemRequest.ProductVariantId,
                        ProductVariantColorId = itemRequest.ProductVariantColorId,
                        InvoicedQuantity = itemRequest.InvoicedQuantity,
                        UnitPrice = itemRequest.UnitPrice,
                        TaxRate = itemRequest.TaxRate,
                        Vehicles = new System.Collections.Generic.List<Vehicle>()
                    };
                    if (itemRequest.Vehicles != null)
                    {
                        foreach (var v in itemRequest.Vehicles)
                        {
                            newItem.Vehicles.Add(new Vehicle
                            {
                                VinNumber = v.VinNumber.Trim(),
                                EngineNumber = v.EngineNumber.Trim(),
                                LicensePlate = string.Empty,
                                ProductVariantId = itemRequest.ProductVariantId,
                                ProductVariantColorId = itemRequest.ProductVariantColorId,
                                LeadId = null,
                                PurchaseDate = DateTimeOffset.UtcNow,
                                IsActive = true,
                                Status = Domain.Constants.Order.VehicleStatus.PendingImport,
                                ImportPrice = v.ImportPrice
                            });
                        }
                    }
                    invoice.PurchaseInvoiceItems.Add(newItem);
                }
            }

            updateRepository.Update(invoice);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var updated = await readRepository.GetByIdWithDetailsAsync(invoice.Id, cancellationToken).ConfigureAwait(false);
            return updated!.Adapt<PurchaseInvoiceDetailResponse?>();
        }
    }
}
