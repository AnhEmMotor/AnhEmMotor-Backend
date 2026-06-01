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
                    src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorName : null);
        }
    }
}
