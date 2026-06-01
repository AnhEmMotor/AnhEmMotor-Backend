using Application.ApiContracts.Quotation.Responses;
using Application.Features.Quotations.Commands.CreateQuotation;
using Application.Features.Quotations.Commands.UpdateQuotation;
using Domain.Entities;
using Mapster;
using System.Linq;

namespace Application.Features.Quotations.Mappings
{
    public sealed class QuotationMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateQuotationCommand, Quotation>()
                .Map(dest => dest.Note, src => src.Notes)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.QuotationProductRows);
            config.NewConfig<UpdateQuotationCommand, Quotation>()
                .Map(dest => dest.Note, src => src.Notes)
                .Ignore(dest => dest.QuotationProductRows)
                .IgnoreNullValues(true);
            config.NewConfig<Quotation, QuotationDetailResponse>()
                .Map(dest => dest.Notes, src => src.Note)
                .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
                .Map(dest => dest.QuotationItems, src => src.QuotationProductRows)
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? src.SentByUser.FullName : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? src.RejectedByUser.FullName : null)
                .Map(dest => dest.LastUpdatedAt, src => src.UpdatedAt ?? src.CreatedAt);
            config.NewConfig<QuotationProductRow, QuotationItemResponse>()
                .Map(
                    dest => dest.ProductVariantDisplayName,
                    src => src.ProductVariant != null
                        ? (src.ProductVariant.Product != null
                            ? $"{src.ProductVariant.Product.Name} {src.ProductVariant.VariantName}".Trim()
                            : src.ProductVariant.VariantName)
                        : null)
                .Map(
                    dest => dest.ProductVariantColorDisplayName,
                    src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorName : null);
            config.NewConfig<Quotation, QuotationSummaryResponse>()
                .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
                .Map(
                    dest => dest.ProductCount,
                    src => src.QuotationProductRows != null ? src.QuotationProductRows.Count : 0)
                .Map(dest => dest.CreatedByName, src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null)
                .Map(dest => dest.SentByName, src => src.SentByUser != null ? src.SentByUser.FullName : null)
                .Map(dest => dest.ApprovedByName, src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null)
                .Map(dest => dest.RejectedByName, src => src.RejectedByUser != null ? src.RejectedByUser.FullName : null)
                .Map(dest => dest.LastUpdatedAt, src => src.UpdatedAt ?? src.CreatedAt);
        }
    }
}
