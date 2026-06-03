using Application.ApiContracts.PurchaseInvoice.Responses;
using Domain.Entities;
using Mapster;
using System.Linq;

namespace Application.Features.PurchaseInvoices.Mappings
{
    public sealed class PurchaseInvoiceMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PurchaseInvoice, PurchaseInvoiceDetailResponse>()
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? (!string.IsNullOrEmpty(src.CreatedByUser.FullName) ? src.CreatedByUser.FullName : src.CreatedByUser.UserName) : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? (!string.IsNullOrEmpty(src.ApprovedByUser.FullName) ? src.ApprovedByUser.FullName : src.ApprovedByUser.UserName) : null)
                .Map(dest => dest.SupplierName, src => src.PurchaseOrder != null && src.PurchaseOrder.Supplier != null ? src.PurchaseOrder.Supplier.Name : null)
                .Map(dest => dest.SupplierId, src => src.PurchaseOrder != null ? (int?)src.PurchaseOrder.SupplierId : null)
                .Map(dest => dest.Items, src => src.PurchaseInvoiceItems)
                .Map(dest => dest.TotalAmountBeforeTax, src => src.PurchaseInvoiceItems != null ? src.PurchaseInvoiceItems.Sum(i => i.InvoicedQuantity * i.UnitPrice) : 0)
                .Map(dest => dest.TotalTaxAmount, src => src.PurchaseInvoiceItems != null ? src.PurchaseInvoiceItems.Sum(i => i.InvoicedQuantity * i.UnitPrice * (i.TaxRate / 100)) : 0)
                .Map(dest => dest.TotalAmount, src => src.PurchaseInvoiceItems != null ? src.PurchaseInvoiceItems.Sum(i => i.InvoicedQuantity * i.UnitPrice * (1 + i.TaxRate / 100)) : 0);

            config.NewConfig<PurchaseInvoice, PurchaseInvoiceListResponse>()
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? (!string.IsNullOrEmpty(src.CreatedByUser.FullName) ? src.CreatedByUser.FullName : src.CreatedByUser.UserName) : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? (!string.IsNullOrEmpty(src.ApprovedByUser.FullName) ? src.ApprovedByUser.FullName : src.ApprovedByUser.UserName) : null)
                .Map(dest => dest.SupplierName, src => src.PurchaseOrder != null && src.PurchaseOrder.Supplier != null ? src.PurchaseOrder.Supplier.Name : null)
                .Map(dest => dest.TotalAmountBeforeTax, src => src.PurchaseInvoiceItems != null ? src.PurchaseInvoiceItems.Sum(i => i.InvoicedQuantity * i.UnitPrice) : 0)
                .Map(dest => dest.TotalTaxAmount, src => src.PurchaseInvoiceItems != null ? src.PurchaseInvoiceItems.Sum(i => i.InvoicedQuantity * i.UnitPrice * (i.TaxRate / 100)) : 0)
                .Map(dest => dest.TotalAmount, src => src.PurchaseInvoiceItems != null ? src.PurchaseInvoiceItems.Sum(i => i.InvoicedQuantity * i.UnitPrice * (1 + i.TaxRate / 100)) : 0);

            config.NewConfig<PurchaseInvoiceItem, PurchaseInvoiceItemResponse>()
                .Map(dest => dest.ProductVariantName, src => src.ProductVariant != null
                    ? (src.ProductVariant.Product != null
                        ? $"{src.ProductVariant.Product.Name} {src.ProductVariant.VariantName}".Trim()
                        : src.ProductVariant.VariantName)
                    : null)
                .Map(dest => dest.SKU, src => src.ProductVariant != null ? src.ProductVariant.SKU : null)
                .Map(dest => dest.ColorName, src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorName : null)
                .Map(dest => dest.NeedVin, src => src.ProductVariant != null && src.ProductVariant.Product != null && src.ProductVariant.Product.ProductCategory != null && src.ProductVariant.Product.ProductCategory.ManagementType == "vin_number")
                .Map(dest => dest.Vehicles, src => src.Vehicles);
        }
    }
}
