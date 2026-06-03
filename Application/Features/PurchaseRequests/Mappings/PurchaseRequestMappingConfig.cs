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
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? (!string.IsNullOrEmpty(src.CreatedByUser.FullName) ? src.CreatedByUser.FullName : src.CreatedByUser.UserName) : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? (!string.IsNullOrEmpty(src.SentByUser.FullName) ? src.SentByUser.FullName : src.SentByUser.UserName) : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? (!string.IsNullOrEmpty(src.ApprovedByUser.FullName) ? src.ApprovedByUser.FullName : src.ApprovedByUser.UserName) : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? (!string.IsNullOrEmpty(src.RejectedByUser.FullName) ? src.RejectedByUser.FullName : src.RejectedByUser.UserName) : null)
                .Map(dest => dest.Items, src => src.PurchaseRequestItems);
            config.NewConfig<PurchaseRequest, PurchaseRequestListResponse>()
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? (!string.IsNullOrEmpty(src.CreatedByUser.FullName) ? src.CreatedByUser.FullName : src.CreatedByUser.UserName) : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? (!string.IsNullOrEmpty(src.SentByUser.FullName) ? src.SentByUser.FullName : src.SentByUser.UserName) : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? (!string.IsNullOrEmpty(src.ApprovedByUser.FullName) ? src.ApprovedByUser.FullName : src.ApprovedByUser.UserName) : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? (!string.IsNullOrEmpty(src.RejectedByUser.FullName) ? src.RejectedByUser.FullName : src.RejectedByUser.UserName) : null)
                .Map(
                    dest => dest.TotalItems,
                    src => src.PurchaseRequestItems != null ? src.PurchaseRequestItems.Count : 0);
            config.NewConfig<PurchaseRequestItem, PurchaseRequestItemResponse>()
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
                .Map(
                    dest => dest.POCreatingQuantity,
                    src => src.PurchaseOrderItems != null
                        ? src.PurchaseOrderItems
                            .Where(poi => poi.DeletedAt == null && 
                                          poi.PurchaseOrder != null && 
                                          poi.PurchaseOrder.DeletedAt == null &&
                                          (string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Draft, StringComparison.OrdinalIgnoreCase) == 0 ||
                                           string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Sent, StringComparison.OrdinalIgnoreCase) == 0))
                            .Sum(poi => poi.OrderedQuantity)
                        : 0)
                .Map(
                    dest => dest.POApprovedQuantity,
                    src => src.PurchaseOrderItems != null
                        ? src.PurchaseOrderItems
                            .Where(poi => poi.DeletedAt == null && 
                                          poi.PurchaseOrder != null && 
                                          poi.PurchaseOrder.DeletedAt == null &&
                                          string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Approved, StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(poi => poi.OrderedQuantity)
                        : 0)
                .Map(
                    dest => dest.PORemainingQuantity,
                    src => src.Quantity - (src.PurchaseOrderItems != null
                        ? src.PurchaseOrderItems
                            .Where(poi => poi.DeletedAt == null && 
                                          poi.PurchaseOrder != null && 
                                          poi.PurchaseOrder.DeletedAt == null &&
                                          (string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Draft, StringComparison.OrdinalIgnoreCase) == 0 ||
                                           string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Sent, StringComparison.OrdinalIgnoreCase) == 0 ||
                                           string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Approved, StringComparison.OrdinalIgnoreCase) == 0))
                            .Sum(poi => poi.OrderedQuantity)
                        : 0))
                .Map(
                    dest => dest.ImportedQuantity,
                    src => src.PurchaseOrderItems != null
                        ? src.PurchaseOrderItems
                            .Where(poi => poi.DeletedAt == null)
                            .SelectMany(poi => poi.InventoryReceiptInfos)
                            .Where(ii => ii.DeletedAt == null &&
                                         ii.InventoryReceipt != null &&
                                         ii.InventoryReceipt.DeletedAt == null &&
                                         string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, System.StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(ii => ii.Count ?? 0)
                        : 0)
                .Map(
                    dest => dest.PendingQuantity,
                    src => src.PurchaseOrderItems != null
                        ? src.PurchaseOrderItems
                            .Where(poi => poi.DeletedAt == null)
                            .SelectMany(poi => poi.InventoryReceiptInfos)
                            .Where(ii => ii.DeletedAt == null &&
                                         ii.InventoryReceipt != null &&
                                         ii.InventoryReceipt.DeletedAt == null &&
                                         (string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Draft, System.StringComparison.OrdinalIgnoreCase) == 0 ||
                                          string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Sent, System.StringComparison.OrdinalIgnoreCase) == 0))
                            .Sum(ii => ii.Count ?? 0)
                        : 0)
                .Map(
                    dest => dest.UnimportedQuantity,
                    src => src.Quantity -
                        (src.PurchaseOrderItems != null
                            ? src.PurchaseOrderItems
                                .Where(poi => poi.DeletedAt == null)
                                .SelectMany(poi => poi.InventoryReceiptInfos)
                                .Where(ii => ii.DeletedAt == null &&
                                             ii.InventoryReceipt != null &&
                                             ii.InventoryReceipt.DeletedAt == null &&
                                             string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, System.StringComparison.OrdinalIgnoreCase) == 0)
                                .Sum(ii => ii.Count ?? 0)
                            : 0));

            config.NewConfig<PurchaseRequest, ApprovedPurchaseRequestDetailResponse>()
                .Map(dest => dest.Items, src => src.PurchaseRequestItems);

            config.NewConfig<PurchaseRequestItem, ApprovedPurchaseRequestItemResponse>()
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
                .Map(
                    dest => dest.POCreatingQuantity,
                    src => src.PurchaseOrderItems != null
                        ? src.PurchaseOrderItems
                            .Where(poi => poi.DeletedAt == null && 
                                          poi.PurchaseOrder != null && 
                                          poi.PurchaseOrder.DeletedAt == null &&
                                          (string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Draft, StringComparison.OrdinalIgnoreCase) == 0 ||
                                           string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Sent, StringComparison.OrdinalIgnoreCase) == 0))
                            .Sum(poi => poi.OrderedQuantity)
                        : 0)
                .Map(
                    dest => dest.POApprovedQuantity,
                    src => src.PurchaseOrderItems != null
                        ? src.PurchaseOrderItems
                            .Where(poi => poi.DeletedAt == null && 
                                          poi.PurchaseOrder != null && 
                                          poi.PurchaseOrder.DeletedAt == null &&
                                          string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Approved, StringComparison.OrdinalIgnoreCase) == 0)
                            .Sum(poi => poi.OrderedQuantity)
                        : 0)
                .Map(
                    dest => dest.PORemainingQuantity,
                    src => src.Quantity - (src.PurchaseOrderItems != null
                        ? src.PurchaseOrderItems
                            .Where(poi => poi.DeletedAt == null && 
                                          poi.PurchaseOrder != null && 
                                          poi.PurchaseOrder.DeletedAt == null &&
                                          (string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Draft, StringComparison.OrdinalIgnoreCase) == 0 ||
                                           string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Sent, StringComparison.OrdinalIgnoreCase) == 0 ||
                                           string.Compare(poi.PurchaseOrder.Status, Domain.Constants.PurchaseOrderStatus.Approved, StringComparison.OrdinalIgnoreCase) == 0))
                            .Sum(poi => poi.OrderedQuantity)
                        : 0))
                .Map(
                    dest => dest.UnimportedQuantity,
                    src => src.Quantity -
                        (src.PurchaseOrderItems != null
                            ? src.PurchaseOrderItems
                                .Where(poi => poi.DeletedAt == null)
                                .SelectMany(poi => poi.InventoryReceiptInfos)
                                .Where(ii => ii.DeletedAt == null &&
                                             ii.InventoryReceipt != null &&
                                             ii.InventoryReceipt.DeletedAt == null &&
                                             string.Compare(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, System.StringComparison.OrdinalIgnoreCase) == 0)
                                .Sum(ii => ii.Count ?? 0)
                            : 0))
                .Map(
                    dest => dest.NeedVin,
                    src => src.ProductVariant != null &&
                           src.ProductVariant.Product != null &&
                           src.ProductVariant.Product.ProductCategory != null &&
                           string.Compare(src.ProductVariant.Product.ProductCategory.ManagementType, Domain.Constants.Product.ProductManagementType.VinNumber, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}

