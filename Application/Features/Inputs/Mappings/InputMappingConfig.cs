using Application.ApiContracts.Input.Requests;
using Application.ApiContracts.Input.Responses;
using Application.ApiContracts.Supplier.Responses;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.UpdateInput;
using Domain.Entities;
using Mapster;

namespace Application.Features.Inputs.Mappings;

public sealed class InputMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateInputCommand, Input>();
        config.NewConfig<CreateInputInfoRequest, InputInfo>();

        config.NewConfig<Input, InputListResponse>()
            .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(
                dest => dest.TotalPayable,
                src => src.InputInfos != null
                    ? src.InputInfos.Sum(ii => (long)(ii.Count ?? 0) * (long)(ii.InputPrice ?? 0))
                    : 0)
            .Map(dest => dest.Products, src => src.InputInfos);

        config.NewConfig<Input, InputDetailResponse>()
            .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null)
            .Map(dest => dest.SupplierPhone, src => src.Supplier != null ? src.Supplier.Phone : null)
            .Map(dest => dest.SupplierEmail, src => src.Supplier != null ? src.Supplier.Email : null)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(
                dest => dest.TotalPayable,
                src => src.InputInfos != null
                    ? src.InputInfos.Sum(ii => (long)(ii.Count ?? 0) * (long)(ii.InputPrice ?? 0))
                    : 0)
            .Map(dest => dest.Products, src => src.InputInfos);

        config.NewConfig<InputInfo, InputInfoResponse>()
            .Map(dest => dest.Name, src => BuildFullVariantName(src.ProductVariant))
            .Map(dest => dest.Quantity, src => src.Count)
            .Map(dest => dest.UnitPrice, src => src.InputPrice)
            .Map(dest => dest.ImportPrice, src => src.InputPrice)
            .Map(dest => dest.Discount, src => 0)
            .Map(dest => dest.Total, src => (decimal)(src.Count ?? 0) * (src.InputPrice ?? 0));

        config.NewConfig<UpdateInputInfoRequest, InputInfo>().IgnoreNullValues(true);

        config.NewConfig<UpdateInputCommand, Input>().IgnoreNullValues(true);

        config.NewConfig<Input, SupplierPurchaseHistoryResponse>()
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(
                dest => dest.TotalPayable,
                src => src.InputInfos != null
                    ? src.InputInfos.Sum(ii => (long)(ii.Count ?? 0) * (long)(ii.InputPrice ?? 0))
                    : 0)
            .Map(dest => dest.TotalItems, src => src.InputInfos != null ? src.InputInfos.Count() : 0);

        config.NewConfig<InputListResponse, SupplierPurchaseHistoryResponse>();
    }

    private static string? BuildFullVariantName(ProductVariant? variant)
    {
        if(variant is null || variant.Product is null)
        {
            return null;
        }

        var productName = variant.Product.Name ?? string.Empty;

        if(variant.VariantOptionValues is null || variant.VariantOptionValues.Count == 0)
        {
            return productName;
        }

        var parts = variant.VariantOptionValues
            .Where(vov => vov.OptionValue is not null && !string.IsNullOrWhiteSpace(vov.OptionValue.Name))
            .Select(
                vov =>
                {
                    var optionName = vov.OptionValue?.Option?.Name;
                    return string.IsNullOrWhiteSpace(optionName)
                        ? vov.OptionValue!.Name
                        : $"{optionName}: {vov.OptionValue!.Name}";
                })
            .ToList();

        if(parts.Count == 0)
        {
            return productName;
        }

        return $"{productName} ({string.Join(", ", parts)})";
    }
}
