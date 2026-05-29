using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.ApiContracts.Supplier.Responses;
using Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceipt;
using Domain.Entities;
using Mapster;

namespace Application.Features.InventoryReceipts.Mappings;

public sealed class InventoryReceiptMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateInventoryReceiptCommand, InventoryReceipt>();
        config.NewConfig<CreateInventoryReceiptInfoRequest, InventoryReceiptInfo>();
        config.NewConfig<InventoryReceipt, InventoryReceiptListResponse>()
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(
                dest => dest.TotalPayable,
                src => src.InventoryReceiptInfos != null
                    ? src.InventoryReceiptInfos.Sum(ii => (long)(ii.Count ?? 0) * (long)(ii.QuotationProductRow != null ? (ii.QuotationProductRow.QuotePrice ?? 0) : 0))
                    : 0)
            .Map(dest => dest.Products, src => src.InventoryReceiptInfos);
        config.NewConfig<InventoryReceipt, InventoryReceiptDetailResponse>()
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(
                dest => dest.TotalPayable,
                src => src.InventoryReceiptInfos != null
                    ? src.InventoryReceiptInfos.Sum(ii => (long)(ii.Count ?? 0) * (long)(ii.QuotationProductRow != null ? (ii.QuotationProductRow.QuotePrice ?? 0) : 0))
                    : 0)
            .Map(dest => dest.Products, src => src.InventoryReceiptInfos);
        config.NewConfig<InventoryReceiptInfo, InventoryReceiptInfoResponse>()
            .Map(
                dest => dest.ProductVariantId,
                src => src.QuotationProductRow != null ? src.QuotationProductRow.ProductVariantId : (src.PurchaseRequestItem != null ? src.PurchaseRequestItem.ProductVariantId : (int?)null))
            .Map(
                dest => dest.ProductVariantColorId,
                src => src.QuotationProductRow != null ? src.QuotationProductRow.ProductVariantColorId : (src.PurchaseRequestItem != null ? src.PurchaseRequestItem.ProductVariantColorId : (int?)null))
            .Map(
                dest => dest.ProductVariantColorName,
                src => src.QuotationProductRow != null && src.QuotationProductRow.ProductVariantColor != null
                    ? src.QuotationProductRow.ProductVariantColor.ColorName
                    : (src.PurchaseRequestItem != null && src.PurchaseRequestItem.ProductVariantColor != null ? src.PurchaseRequestItem.ProductVariantColor.ColorName : null))
            .Map(
                dest => dest.SupplierId,
                src => src.QuotationProductRow != null && src.QuotationProductRow.QuotationReceipt != null ? src.QuotationProductRow.QuotationReceipt.SupplierId : (int?)null)
            .Map(
                dest => dest.SupplierName,
                src => src.QuotationProductRow != null && src.QuotationProductRow.QuotationReceipt != null && src.QuotationProductRow.QuotationReceipt.Supplier != null
                    ? src.QuotationProductRow.QuotationReceipt.Supplier.Name
                    : null)
            .Map(
                dest => dest.Name,
                src => BuildFullVariantName(src.QuotationProductRow != null ? src.QuotationProductRow.ProductVariant : (src.PurchaseRequestItem != null ? src.PurchaseRequestItem.ProductVariant : null)))
            .Map(dest => dest.Quantity, src => src.Count)
            .Map(
                dest => dest.UnitPrice,
                src => src.QuotationProductRow != null ? src.QuotationProductRow.QuotePrice : 0)
            .Map(
                dest => dest.ImportPrice,
                src => src.QuotationProductRow != null ? src.QuotationProductRow.QuotePrice : 0)
            .Map(dest => dest.Discount, src => 0)
            .Map(
                dest => dest.Total,
                src => (decimal)(src.Count ?? 0) * (src.QuotationProductRow != null ? (src.QuotationProductRow.QuotePrice ?? 0) : 0))
            .Map(dest => dest.Vehicles, src => src.Vehicles);
        config.NewConfig<UpdateInventoryReceiptInfoRequest, InventoryReceiptInfo>()
            .Ignore(dest => dest.Vehicles)
            .IgnoreNullValues(true);
        config.NewConfig<UpdateInventoryReceiptCommand, InventoryReceipt>().IgnoreNullValues(true);
        config.NewConfig<Vehicle, InventoryReceiptVehicleResponse>()
            .Map(dest => dest.ProductVariantId, src => src.ProductVariantId)
            .Map(dest => dest.ProductVariantColorId, src => src.ProductVariantColorId);
        config.NewConfig<InventoryReceipt, SupplierPurchaseHistoryResponse>()
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(
                dest => dest.TotalPayable,
                src => src.InventoryReceiptInfos != null
                    ? src.InventoryReceiptInfos.Sum(ii => (long)(ii.Count ?? 0) * (long)(ii.QuotationProductRow != null ? (ii.QuotationProductRow.QuotePrice ?? 0) : 0))
                    : 0)
            .Map(dest => dest.TotalItems, src => src.InventoryReceiptInfos != null ? src.InventoryReceiptInfos.Count() : 0);
        config.NewConfig<InventoryReceiptListResponse, SupplierPurchaseHistoryResponse>();
    }

    private static string? BuildFullVariantName(ProductVariant? variant)
    {
        if (variant is null || variant.Product is null)
        {
            return null;
        }
        var productName = variant.Product.Name ?? string.Empty;
        if (string.IsNullOrWhiteSpace(variant.VariantName))
        {
            return productName;
        }
        return $"{productName} ({variant.VariantName})";
    }
}
