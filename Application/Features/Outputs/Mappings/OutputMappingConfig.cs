using Application.ApiContracts.Output.Requests;
using Application.ApiContracts.Output.Responses;
using Application.Features.Outputs.Commands.CreateOutputByManager;
using Application.Features.Outputs.Commands.UpdateOutputForManager;
using Domain.Entities;
using Mapster;

namespace Application.Features.Outputs.Mappings;

public sealed class OutputMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Output, OrderDetailResponse>()
            .Map(dest => dest.Total, src => CalculateTotal(src))
            .Map(dest => dest.ShippingFee, src => CalculateShippingFee(src))
            .Map(dest => dest.DepositAmount, src => CalculateDeposit(src))
            .Map(dest => dest.RemainingAmount, src => CalculateRemaining(src))
            .Map(dest => dest.BuyerName, src => src.Buyer != null ? src.Buyer.FullName : null)
            .Map(dest => dest.BuyerPhone, src => src.Buyer != null ? src.Buyer.PhoneNumber : null)
            .Map(dest => dest.BuyerEmail, src => src.Buyer != null ? src.Buyer.Email : null)
            .Map(
                dest => dest.CompletedByUserName,
                src => src.FinishedByUser != null ? src.FinishedByUser.FullName : null)
            .Map(dest => dest.CreatedByUserId, src => src.CreatedBy)
            .Map(dest => dest.Products, src => src.OutputInfos);
        config.NewConfig<Output, OutputItemResponse>()
            .Map(dest => dest.BuyerName, src => src.Buyer != null ? src.Buyer.FullName : null)
            .Map(dest => dest.BuyerEmail, src => src.Buyer != null ? src.Buyer.Email : null)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.StatusId, src => src.StatusId)
            .Map(dest => dest.Total, src => CalculateTotal(src))
            .Map(dest => dest.DepositAmount, src => CalculateDeposit(src))
            .Map(dest => dest.RemainingAmount, src => CalculateRemaining(src));
        config.NewConfig<Output, MyOrderResponse>()
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.OutputInfos, src => src.OutputInfos)
            .Map(dest => dest.Total, src => CalculateTotal(src))
            .Map(dest => dest.DepositAmount, src => CalculateDeposit(src))
            .Map(dest => dest.RemainingAmount, src => CalculateRemaining(src));
        config.NewConfig<OutputInfo, MyOrderItemResponse>()
            .Map(dest => dest.ProductName, src => MapProductName(src))
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.CoverImageUrl, src => MapCoverImageUrl(src.ProductVariant));
        config.NewConfig<OutputInfo, OutputInfoResponse>()
            .Map(dest => dest.ProductVariantId, src => src.ProductVariantId)
            .Map(dest => dest.ProductVariantColorId, src => src.ProductVariantColorId)
            .Map(dest => dest.ProductName, src => MapProductName(src))
            .Map(dest => dest.CoverImageUrl, src => MapCoverImageUrl(src.ProductVariant))
            .Map(dest => dest.AssignedVehicles, src => MapAssignedVehicles(src));
        config.NewConfig<CreateOutputInfoRequest, OutputInfo>()
            .Map(dest => dest.ProductVariantId, src => src.ProductVariantId)
            .Map(dest => dest.ProductVariantColorId, src => src.ProductVariantColorId)
            .IgnoreNullValues(true);
        config.NewConfig<UpdateOutputInfoRequest, OutputInfo>()
            .Map(dest => dest.ProductVariantId, src => src.ProductVariantId)
            .Map(dest => dest.ProductVariantColorId, src => src.ProductVariantColorId)
            .Map(dest => dest.Count, src => src.Count)
            .Ignore(dest => dest.Id)
            .IgnoreNullValues(true);
        config.NewConfig<UpdateOutputForManagerCommand, Output>()
            .Map(dest => dest.CreatedBy, src => src.CurrentUserId)
            .IgnoreNullValues(true)
            .Ignore(dest => dest.OutputInfos);
        config.NewConfig<CreateOutputByManagerCommand, Output>()
            .Map(dest => dest.CreatedBy, src => src.CurrentUserId)
            .IgnoreNullValues(true);
    }

    private static decimal CalculateTotal(Output src)
    {
        var subtotal = src.OutputInfos?.Sum(oi => (oi.Count ?? 0) * (oi.Price ?? 0)) ?? 0;
        var shipping = CalculateShippingFee(src);
        return subtotal + shipping;
    }

    private static decimal CalculateShippingFee(Output src)
    {
        var subtotal = src.OutputInfos?.Sum(oi => (oi.Count ?? 0) * (oi.Price ?? 0)) ?? 0;
        if (subtotal == 0)
            return 0;
        return subtotal > 10000000 ? 0 : 200000;
    }

    private static decimal? CalculateDeposit(Output src)
    {
        if (src.DepositRatio == null || src.DepositRatio == 0)
            return null;
        var total = CalculateTotal(src);
        return total * (src.DepositRatio.Value / 100m);
    }

    private static decimal? CalculateRemaining(Output src)
    {
        var total = CalculateTotal(src);
        var deposit = CalculateDeposit(src) ?? 0;
        return total - deposit;
    }

    private static string? MapProductName(OutputInfo src)
    {
        if (src.ProductVariant?.Product is null)
            return null;
        var productName = src.ProductVariant.Product.Name;
        var optionValues = src.ProductVariant.VariantOptionValues?
            .Select(vov => vov.OptionValue?.Name).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
        if (optionValues is null || optionValues.Count == 0)
            return productName;
        return $"{productName} ({string.Join(" - ", optionValues)})";
    }

    private static string? MapCoverImageUrl(ProductVariant? variant)
    {
        if (variant == null)
            return null;
        if (variant.ProductVariantColor != null && !string.IsNullOrEmpty(variant.ProductVariantColor.CoverImageUrl))
            return variant.ProductVariantColor.CoverImageUrl;
        if (!string.IsNullOrEmpty(variant.CoverImageUrl))
            return variant.CoverImageUrl;
        return variant.ProductCollectionPhotos?
            .OrderBy(p => p.Id).Select(p => p.ImageUrl).FirstOrDefault();
    }

    private static List<VehicleAssignmentOptionResponse> MapAssignedVehicles(OutputInfo src)
    {
        return src.Vehicles?
            .Select(
                v => new VehicleAssignmentOptionResponse
                {
                    Id = v.Id,
                    VinNumber = v.VinNumber,
                    EngineNumber = v.EngineNumber,
                    Status = v.Status
                })
                .ToList() ??
            [];
    }
}
