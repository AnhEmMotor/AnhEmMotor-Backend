using Application.ApiContracts.PurchaseRequest.Responses;
using Domain.Entities;
using Mapster;
using System.Linq;

namespace Application.Features.PurchaseRequests.Mappings
{
    public sealed class PurchaseRequestMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PurchaseRequest, PurchaseRequestDetailResponse>()
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null)
                .Map(dest => dest.Items, src => src.PurchaseRequestItems);

            config.NewConfig<PurchaseRequest, PurchaseRequestListResponse>()
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null)
                .Map(dest => dest.TotalItems, src => src.PurchaseRequestItems != null ? src.PurchaseRequestItems.Count : 0);

            config.NewConfig<PurchaseRequestItem, PurchaseRequestItemResponse>()
                .Map(dest => dest.ProductName, src => src.ProductVariant != null
                    ? (src.ProductVariant.Product != null
                        ? $"{src.ProductVariant.Product.Name} {src.ProductVariant.VariantName}".Trim()
                        : src.ProductVariant.VariantName)
                    : null)
                .Map(dest => dest.ProductVariantColorName, src => src.ProductVariantColor != null
                    ? src.ProductVariantColor.ColorName
                    : null)
                .Map(dest => dest.ImportedQuantity, src => src.InventoryReceiptInfos != null
                    ? src.InventoryReceiptInfos.Where(ii => ii.InventoryReceiptReceipt != null && (string.Compare(ii.InventoryReceiptReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve) == 0)).Sum(ii => ii.Count ?? 0)
                    : 0)
                .Map(dest => dest.PendingQuantity, src => src.InventoryReceiptInfos != null
                    ? src.InventoryReceiptInfos.Where(ii => ii.InventoryReceiptReceipt != null && (string.Compare(ii.InventoryReceiptReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Draft) == 0 || string.Compare(ii.InventoryReceiptReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Sent) == 0 || string.Compare(ii.InventoryReceiptReceipt.StatusId, "working") == 0)).Sum(ii => ii.Count ?? 0)
                    : 0)
                .Map(dest => dest.UnimportedQuantity, src => src.Quantity - (src.InventoryReceiptInfos != null
                    ? src.InventoryReceiptInfos.Where(ii => ii.InventoryReceiptReceipt != null && (string.Compare(ii.InventoryReceiptReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve) == 0)).Sum(ii => ii.Count ?? 0)
                    : 0));
        }
    }
}
