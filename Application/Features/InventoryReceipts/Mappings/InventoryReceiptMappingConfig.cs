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
                .Map(dest => dest.SupplierId, src => src.InventoryReceiptInfos.FirstOrDefault(x => x.SupplierId.HasValue) != null ? src.InventoryReceiptInfos.FirstOrDefault(x => x.SupplierId.HasValue)!.SupplierId : (int?)null)
                .Map(dest => dest.SupplierName, src => src.InventoryReceiptInfos.FirstOrDefault(x => x.SupplierId.HasValue) != null && src.InventoryReceiptInfos.FirstOrDefault(x => x.SupplierId.HasValue)!.Supplier != null ? src.InventoryReceiptInfos.FirstOrDefault(x => x.SupplierId.HasValue)!.Supplier!.Name : null)
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? src.SentByUser.FullName : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? src.RejectedByUser.FullName : null)
                .Map(dest => dest.Products, src => src.InventoryReceiptInfos);

            config.NewConfig<InventoryReceipt, InventoryReceiptDetailResponse>()
                .Map(dest => dest.SupplierId, src => src.InventoryReceiptInfos.FirstOrDefault(x => x.SupplierId.HasValue) != null ? src.InventoryReceiptInfos.FirstOrDefault(x => x.SupplierId.HasValue)!.SupplierId : (int?)null)
                .Map(dest => dest.SupplierName, src => src.InventoryReceiptInfos.FirstOrDefault(x => x.SupplierId.HasValue) != null && src.InventoryReceiptInfos.FirstOrDefault(x => x.SupplierId.HasValue)!.Supplier != null ? src.InventoryReceiptInfos.FirstOrDefault(x => x.SupplierId.HasValue)!.Supplier!.Name : null)
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? src.SentByUser.FullName : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? src.RejectedByUser.FullName : null)
                .Map(dest => dest.Products, src => src.InventoryReceiptInfos);

            config.NewConfig<InventoryReceiptInfo, InventoryReceiptInfoResponse>()
                .Map(dest => dest.ProductVariantId, src => src.PurchaseRequestItem != null ? src.PurchaseRequestItem.ProductVariantId : (int?)null)
                .Map(dest => dest.ProductVariantColorId, src => src.PurchaseRequestItem != null ? src.PurchaseRequestItem.ProductVariantColorId : (int?)null)
                .Map(dest => dest.ProductVariantColorName, src => src.PurchaseRequestItem != null && src.PurchaseRequestItem.ProductVariantColor != null ? src.PurchaseRequestItem.ProductVariantColor.ColorName : null)
                .Map(dest => dest.SupplierId, src => src.SupplierId)
                .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
                .Map(dest => dest.Name, src => src.PurchaseRequestItem != null && src.PurchaseRequestItem.ProductVariant != null && src.PurchaseRequestItem.ProductVariant.Product != null ? src.PurchaseRequestItem.ProductVariant.Product.Name : null)
                .Map(dest => dest.Quantity, src => src.Count)
                .Map(dest => dest.UnitPrice, src => src.UnitPrice)
                .Map(dest => dest.OrderedQuantity, src => src.PurchaseRequestItem != null ? src.PurchaseRequestItem.Quantity : (int?)null)
                .Map(dest => dest.MaxAllowedQuantity, src => src.PurchaseRequestItem != null
                    ? src.PurchaseRequestItem.Quantity - src.PurchaseRequestItem.InventoryReceiptInfos
                        .Where(ii => ii.DeletedAt == null &&
                                     ii.Id != src.Id &&
                                     ii.InventoryReceipt != null &&
                                     ii.InventoryReceipt.DeletedAt == null &&
                                     (string.Equals(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, System.StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Sent, System.StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Draft, System.StringComparison.OrdinalIgnoreCase)))
                        .Sum(ii => ii.Count ?? 0)
                    : (int?)null)
                .Map(dest => dest.PurchaseRequestItemId, src => src.PurchaseRequestItemId)
                .Map(dest => dest.Vehicles, src => src.Vehicles);
        }
    }
}

