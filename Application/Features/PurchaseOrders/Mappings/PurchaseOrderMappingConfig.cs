using Application.ApiContracts.PurchaseOrder.Responses;
using Domain.Entities;
using Mapster;
using System.Linq;

namespace Application.Features.PurchaseOrders.Mappings
{
    public sealed class PurchaseOrderMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PurchaseOrder, PurchaseOrderDetailResponse>()
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? (!string.IsNullOrEmpty(src.CreatedByUser.FullName) ? src.CreatedByUser.FullName : src.CreatedByUser.UserName) : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? (!string.IsNullOrEmpty(src.SentByUser.FullName) ? src.SentByUser.FullName : src.SentByUser.UserName) : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? (!string.IsNullOrEmpty(src.ApprovedByUser.FullName) ? src.ApprovedByUser.FullName : src.ApprovedByUser.UserName) : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? (!string.IsNullOrEmpty(src.RejectedByUser.FullName) ? src.RejectedByUser.FullName : src.RejectedByUser.UserName) : null)
                .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
                .Map(dest => dest.Items, src => src.PurchaseOrderItems)
                .Map(dest => dest.TotalAmount, src => src.PurchaseOrderItems != null ? src.PurchaseOrderItems.Where(item => item.DeletedAt == null).Sum(item => item.OrderedQuantity * item.UnitPrice) : 0);

            config.NewConfig<PurchaseOrder, PurchaseOrderListResponse>()
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? (!string.IsNullOrEmpty(src.CreatedByUser.FullName) ? src.CreatedByUser.FullName : src.CreatedByUser.UserName) : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? (!string.IsNullOrEmpty(src.SentByUser.FullName) ? src.SentByUser.FullName : src.SentByUser.UserName) : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? (!string.IsNullOrEmpty(src.ApprovedByUser.FullName) ? src.ApprovedByUser.FullName : src.ApprovedByUser.UserName) : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? (!string.IsNullOrEmpty(src.RejectedByUser.FullName) ? src.RejectedByUser.FullName : src.RejectedByUser.UserName) : null)
                .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
                .Map(
                    dest => dest.TotalItems,
                    src => src.PurchaseOrderItems != null ? src.PurchaseOrderItems.Where(item => item.DeletedAt == null).Count() : 0)
                .Map(
                    dest => dest.TotalAmount,
                    src => src.PurchaseOrderItems != null ? src.PurchaseOrderItems.Where(item => item.DeletedAt == null).Sum(item => item.OrderedQuantity * item.UnitPrice) : 0);

            config.NewConfig<PurchaseOrderItem, PurchaseOrderItemResponse>()
                .Map(
                    dest => dest.ProductName,
                    src => src.ProductVariant != null
                        ? (src.ProductVariant.Product != null
                            ? $"{src.ProductVariant.Product.Name} {src.ProductVariant.VariantName}".Trim()
                            : src.ProductVariant.VariantName)
                        : null)
                .Map(
                    dest => dest.ProductVariantColorName,
                    src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorName : null)
                .Map(dest => dest.QuotationProductRowId, src => src.QuotationProductRowId)
                .Map(dest => dest.PurchaseRequestItemId, src => src.PurchaseRequestItemId)
                .Map(dest => dest.QuotationId, src => src.QuotationProductRow != null ? src.QuotationProductRow.QuotationId : null)
                .Map(dest => dest.QuotationName, src => src.QuotationProductRow != null ? $"Báo giá #{src.QuotationProductRow.QuotationId}" : null);

            config.NewConfig<PurchaseOrder, PurchaseOrderDetailForInputResponse>()
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? (!string.IsNullOrEmpty(src.CreatedByUser.FullName) ? src.CreatedByUser.FullName : src.CreatedByUser.UserName) : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? (!string.IsNullOrEmpty(src.SentByUser.FullName) ? src.SentByUser.FullName : src.SentByUser.UserName) : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? (!string.IsNullOrEmpty(src.ApprovedByUser.FullName) ? src.ApprovedByUser.FullName : src.ApprovedByUser.UserName) : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? (!string.IsNullOrEmpty(src.RejectedByUser.FullName) ? src.RejectedByUser.FullName : src.RejectedByUser.UserName) : null)
                .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
                .Map(dest => dest.Items, src => src.PurchaseOrderItems)
                .Map(dest => dest.TotalAmount, src => src.PurchaseOrderItems != null ? src.PurchaseOrderItems.Where(item => item.DeletedAt == null).Sum(item => item.OrderedQuantity * item.UnitPrice) : 0);

            config.NewConfig<PurchaseOrderItem, PurchaseOrderItemForInputResponse>()
                .Map(
                    dest => dest.ProductName,
                    src => src.ProductVariant != null
                        ? (src.ProductVariant.Product != null
                            ? $"{src.ProductVariant.Product.Name} {src.ProductVariant.VariantName}".Trim()
                            : src.ProductVariant.VariantName)
                        : null)
                .Map(
                    dest => dest.ProductVariantColorName,
                    src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorName : null)
                .Map(dest => dest.QuotationProductRowId, src => src.QuotationProductRowId)
                .Map(dest => dest.PurchaseRequestItemId, src => src.PurchaseRequestItemId)
                .Map(dest => dest.QuotationId, src => src.QuotationProductRow != null ? src.QuotationProductRow.QuotationId : null)
                .Map(dest => dest.QuotationName, src => src.QuotationProductRow != null ? $"Báo giá #{src.QuotationProductRow.QuotationId}" : null)
                .Map(
                    dest => dest.NeedVin,
                    src => src.ProductVariant != null &&
                           src.ProductVariant.Product != null &&
                           src.ProductVariant.Product.ProductCategory != null &&
                           string.Compare(src.ProductVariant.Product.ProductCategory.ManagementType, Domain.Constants.Product.ProductManagementType.VinNumber, StringComparison.OrdinalIgnoreCase) == 0)
                .Map(
                    dest => dest.ImportedQuantity,
                    src => src.InventoryReceiptInfos != null
                        ? src.InventoryReceiptInfos
                            .Where(ii => ii.DeletedAt == null && 
                                         ii.InventoryReceipt != null && 
                                         ii.InventoryReceipt.DeletedAt == null && 
                                         string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, System.StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(ii => ii.Count ?? 0)
                        : 0)
                .Map(
                    dest => dest.SentQuantity,
                    src => src.InventoryReceiptInfos != null
                        ? src.InventoryReceiptInfos
                            .Where(ii => ii.DeletedAt == null && 
                                         ii.InventoryReceipt != null && 
                                         ii.InventoryReceipt.DeletedAt == null && 
                                         string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Sent, System.StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(ii => ii.Count ?? 0)
                        : 0)
                .Map(
                    dest => dest.DraftQuantity,
                    src => src.InventoryReceiptInfos != null
                        ? src.InventoryReceiptInfos
                            .Where(ii => ii.DeletedAt == null && 
                                         ii.InventoryReceipt != null && 
                                         ii.InventoryReceipt.DeletedAt == null && 
                                         string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Draft, System.StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(ii => ii.Count ?? 0)
                        : 0)
                .Map(
                    dest => dest.RemainingQuantity,
                    src => src.OrderedQuantity - (src.InventoryReceiptInfos != null
                        ? src.InventoryReceiptInfos
                            .Where(ii => ii.DeletedAt == null && 
                                         ii.InventoryReceipt != null && 
                                         ii.InventoryReceipt.DeletedAt == null && 
                                         (string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, System.StringComparison.OrdinalIgnoreCase) == 0 ||
                                          string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Sent, System.StringComparison.OrdinalIgnoreCase) == 0 ||
                                          string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Draft, System.StringComparison.OrdinalIgnoreCase) == 0))
                            .Sum(ii => ii.Count ?? 0)
                        : 0))
                .Map(
                    dest => dest.InvoicedVehicles,
                    src => src.PurchaseInvoiceItems != null
                        ? src.PurchaseInvoiceItems
                            .Where(pii => pii.DeletedAt == null &&
                                          pii.PurchaseInvoice != null &&
                                          pii.PurchaseInvoice.DeletedAt == null &&
                                          (string.Compare(pii.PurchaseInvoice.Status, "approved", System.StringComparison.OrdinalIgnoreCase) == 0 ||
                                           string.Compare(pii.PurchaseInvoice.Status, "draft", System.StringComparison.OrdinalIgnoreCase) == 0))
                            .SelectMany(pii => pii.Vehicles.Where(v => v.DeletedAt == null && v.InventoryReceiptInfoId == null &&
                                !(src.InventoryReceiptInfos != null && src.InventoryReceiptInfos
                                    .Where(ii => ii.DeletedAt == null &&
                                                 ii.InventoryReceipt != null &&
                                                 ii.InventoryReceipt.DeletedAt == null &&
                                                 string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Reject, System.StringComparison.OrdinalIgnoreCase) != 0)
                                    .SelectMany(ii => ii.Vehicles.Where(veh => veh.DeletedAt == null))
                                    .Any(veh => string.Equals(veh.VinNumber.Trim(), v.VinNumber.Trim(), System.StringComparison.OrdinalIgnoreCase)))))
                            .ToList()
                        : new System.Collections.Generic.List<Vehicle>());

            config.NewConfig<Vehicle, PurchaseOrderVehicleInputResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.VinNumber, src => src.VinNumber)
                .Map(dest => dest.EngineNumber, src => src.EngineNumber)
                .Map(dest => dest.PurchaseInvoiceItemId, src => src.PurchaseInvoiceItemId);

            config.NewConfig<PurchaseOrder, PurchaseOrderDetailForInvoiceResponse>()
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? (!string.IsNullOrEmpty(src.CreatedByUser.FullName) ? src.CreatedByUser.FullName : src.CreatedByUser.UserName) : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? (!string.IsNullOrEmpty(src.SentByUser.FullName) ? src.SentByUser.FullName : src.SentByUser.UserName) : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? (!string.IsNullOrEmpty(src.ApprovedByUser.FullName) ? src.ApprovedByUser.FullName : src.ApprovedByUser.UserName) : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? (!string.IsNullOrEmpty(src.RejectedByUser.FullName) ? src.RejectedByUser.FullName : src.RejectedByUser.UserName) : null)
                .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
                .Map(dest => dest.Items, src => src.PurchaseOrderItems)
                .Map(dest => dest.TotalAmount, src => src.PurchaseOrderItems != null ? src.PurchaseOrderItems.Where(item => item.DeletedAt == null).Sum(item => item.OrderedQuantity * item.UnitPrice) : 0);

            config.NewConfig<Vehicle, PurchaseOrderVehicleInvoiceResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.VinNumber, src => src.VinNumber)
                .Map(dest => dest.EngineNumber, src => src.EngineNumber)
                .Map(dest => dest.ImportPrice, src => src.ImportPrice)
                .Map(dest => dest.InventoryReceiptInfoId, src => src.InventoryReceiptInfoId);

            config.NewConfig<PurchaseOrderItem, PurchaseOrderItemForInvoiceResponse>()
                .Map(
                    dest => dest.ProductName,
                    src => src.ProductVariant != null
                        ? (src.ProductVariant.Product != null
                            ? $"{src.ProductVariant.Product.Name} {src.ProductVariant.VariantName}".Trim()
                            : src.ProductVariant.VariantName)
                        : null)
                .Map(
                    dest => dest.ProductVariantColorName,
                    src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorName : null)
                .Map(dest => dest.QuotationProductRowId, src => src.QuotationProductRowId)
                .Map(dest => dest.PurchaseRequestItemId, src => src.PurchaseRequestItemId)
                .Map(dest => dest.QuotationId, src => src.QuotationProductRow != null ? src.QuotationProductRow.QuotationId : null)
                .Map(dest => dest.QuotationName, src => src.QuotationProductRow != null ? $"Báo giá #{src.QuotationProductRow.QuotationId}" : null)
                .Map(
                    dest => dest.NeedVin,
                    src => src.ProductVariant != null &&
                           src.ProductVariant.Product != null &&
                           src.ProductVariant.Product.ProductCategory != null &&
                           string.Compare(src.ProductVariant.Product.ProductCategory.ManagementType, Domain.Constants.Product.ProductManagementType.VinNumber, StringComparison.OrdinalIgnoreCase) == 0)
                .Map(
                    dest => dest.InvoicedQuantity,
                    src => src.PurchaseInvoiceItems != null
                        ? src.PurchaseInvoiceItems
                            .Where(pii => pii.DeletedAt == null &&
                                          pii.PurchaseInvoice != null &&
                                          pii.PurchaseInvoice.DeletedAt == null &&
                                          string.Compare(pii.PurchaseInvoice.Status, "approved", System.StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(pii => pii.InvoicedQuantity)
                        : 0)
                .Map(
                    dest => dest.InvoicingQuantity,
                    src => src.PurchaseInvoiceItems != null
                        ? src.PurchaseInvoiceItems
                            .Where(pii => pii.DeletedAt == null &&
                                          pii.PurchaseInvoice != null &&
                                          pii.PurchaseInvoice.DeletedAt == null &&
                                          string.Compare(pii.PurchaseInvoice.Status, "draft", System.StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(pii => pii.InvoicedQuantity)
                        : 0)
                .Map(
                    dest => dest.RemainingQuantity,
                    src => src.OrderedQuantity - (src.PurchaseInvoiceItems != null
                        ? src.PurchaseInvoiceItems
                            .Where(pii => pii.DeletedAt == null &&
                                          pii.PurchaseInvoice != null &&
                                          pii.PurchaseInvoice.DeletedAt == null &&
                                          (string.Compare(pii.PurchaseInvoice.Status, "approved", System.StringComparison.OrdinalIgnoreCase) == 0 ||
                                           string.Compare(pii.PurchaseInvoice.Status, "draft", System.StringComparison.OrdinalIgnoreCase) == 0))
                            .Sum(pii => pii.InvoicedQuantity)
                        : 0))
                .Map(
                    dest => dest.ImportedVehicles,
                    src => src.InventoryReceiptInfos != null
                        ? src.InventoryReceiptInfos
                            .Where(ii => ii.DeletedAt == null &&
                                         ii.InventoryReceipt != null &&
                                         ii.InventoryReceipt.DeletedAt == null &&
                                         (string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, System.StringComparison.OrdinalIgnoreCase) == 0 ||
                                          string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Sent, System.StringComparison.OrdinalIgnoreCase) == 0 ||
                                          string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Draft, System.StringComparison.OrdinalIgnoreCase) == 0))
                            .SelectMany(ii => ii.Vehicles.Where(v => v.DeletedAt == null &&
                                !(src.PurchaseInvoiceItems != null && src.PurchaseInvoiceItems
                                    .Where(pii => pii.DeletedAt == null &&
                                                  pii.PurchaseInvoice != null &&
                                                  pii.PurchaseInvoice.DeletedAt == null &&
                                                  string.Compare(pii.PurchaseInvoice.Status, "rejected", System.StringComparison.OrdinalIgnoreCase) != 0)
                                    .SelectMany(pii => pii.Vehicles.Where(veh => veh.DeletedAt == null))
                                    .Any(veh => string.Equals(veh.VinNumber.Trim(), v.VinNumber.Trim(), System.StringComparison.OrdinalIgnoreCase)))))
                            .ToList()
                        : new System.Collections.Generic.List<Vehicle>());
        }
    }
}
