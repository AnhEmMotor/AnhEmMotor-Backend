using Mapster;
using Domain.Entities;
using Application.ApiContracts.InventoryReceipt.Responses;

namespace Application.Features.InventoryReceipts.Mappings
{
    public sealed class InventoryReceiptMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<InventoryReceipt, InventoryReceiptListResponse>()
                .Map(dest => dest.SupplierId, src => src.PurchaseOrder != null ? src.PurchaseOrder.SupplierId : (int?)null)
                .Map(dest => dest.SupplierName, src => src.PurchaseOrder != null && src.PurchaseOrder.Supplier != null ? src.PurchaseOrder.Supplier.Name : null)
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? src.SentByUser.FullName : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? src.RejectedByUser.FullName : null)
                .Map(dest => dest.Products, src => src.InventoryReceiptInfos);

            config.NewConfig<InventoryReceipt, InventoryReceiptDetailResponse>()
                .Map(dest => dest.SupplierId, src => src.PurchaseOrder != null ? src.PurchaseOrder.SupplierId : (int?)null)
                .Map(dest => dest.SupplierName, src => src.PurchaseOrder != null && src.PurchaseOrder.Supplier != null ? src.PurchaseOrder.Supplier.Name : null)
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? src.SentByUser.FullName : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? src.RejectedByUser.FullName : null)
                .Map(dest => dest.Products, src => src.InventoryReceiptInfos);

            config.NewConfig<InventoryReceiptInfo, InventoryReceiptInfoResponse>()
                .Map(dest => dest.ProductVariantId, src => src.PurchaseOrderItem != null ? src.PurchaseOrderItem.ProductVariantId : (int?)null)
                .Map(dest => dest.ProductVariantColorId, src => src.PurchaseOrderItem != null ? src.PurchaseOrderItem.ProductVariantColorId : (int?)null)
                .Map(dest => dest.ProductVariantColorName, src => src.PurchaseOrderItem != null && src.PurchaseOrderItem.ProductVariantColor != null ? src.PurchaseOrderItem.ProductVariantColor.ColorName : null)
                .Map(dest => dest.SupplierId, src => src.InventoryReceipt != null && src.InventoryReceipt.PurchaseOrder != null ? src.InventoryReceipt.PurchaseOrder.SupplierId : (int?)null)
                .Map(dest => dest.SupplierName, src => src.InventoryReceipt != null && src.InventoryReceipt.PurchaseOrder != null && src.InventoryReceipt.PurchaseOrder.Supplier != null ? src.InventoryReceipt.PurchaseOrder.Supplier.Name : null)
                .Map(dest => dest.Name, src => src.PurchaseOrderItem != null && src.PurchaseOrderItem.ProductVariant != null && src.PurchaseOrderItem.ProductVariant.Product != null ? src.PurchaseOrderItem.ProductVariant.Product.Name : null)
                .Map(dest => dest.Quantity, src => src.Count)
                .Map(dest => dest.OrderedQuantity, src => src.PurchaseOrderItem != null ? src.PurchaseOrderItem.OrderedQuantity : (int?)null)
                .Map(dest => dest.MaxAllowedQuantity, src => src.PurchaseOrderItem != null
                    ? src.PurchaseOrderItem.OrderedQuantity - src.PurchaseOrderItem.InventoryReceiptInfos
                        .Where(ii => ii.DeletedAt == null &&
                                     ii.Id != src.Id &&
                                     ii.InventoryReceipt != null &&
                                     ii.InventoryReceipt.DeletedAt == null &&
                                     (string.Equals(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, System.StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Sent, System.StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Draft, System.StringComparison.OrdinalIgnoreCase)))
                        .Sum(ii => ii.Count ?? 0)
                    : (int?)null)
                .Map(dest => dest.PurchaseOrderItemId, src => src.PurchaseOrderItemId)
                .Map(dest => dest.Vehicles, src => src.Vehicles);
        }
    }
}

