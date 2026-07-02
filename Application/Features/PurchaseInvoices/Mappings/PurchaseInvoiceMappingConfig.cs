using Application.ApiContracts.PurchaseInvoice.Responses;
using Domain.Entities;
using Mapster;

namespace Application.Features.PurchaseInvoices.Mappings
{
    public class PurchaseInvoiceMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PurchaseInvoice, PurchaseInvoiceListResponse>()
                .Map(dest => dest.TotalItems, src => src.PurchaseInvoiceItems.Count);

            config.NewConfig<PurchaseInvoice, PurchaseInvoiceDetailResponse>()
                .Map(dest => dest.Items, src => src.PurchaseInvoiceItems);

            config.NewConfig<PurchaseInvoiceItem, PurchaseInvoiceItemResponse>()
                .Map(dest => dest.ProductName, src => src.ProductName)
                .Map(dest => dest.VariantName, src => src.VariantName)
                .Map(dest => dest.ColorName, src => src.ColorName);
        }
    }
}
