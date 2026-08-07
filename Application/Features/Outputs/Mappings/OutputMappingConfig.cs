using Application.ApiContracts.Output.Requests;
using Application.ApiContracts.Output.Responses;
using Application.Features.Outputs.Commands.CreateOutputByManager;
using Application.Features.Outputs.Commands.UpdateOutputForManager;
using Domain.Entities;
using Mapster;

namespace Application.Features.Outputs.Mappings;

public class OutputMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Output, OrderDetailResponse>()
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.Total, src => CalculateTotal(src))
            .Map(dest => dest.Subtotal, src => CalculateSubtotal(src))
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
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.Total, src => CalculateTotal(src))
            .Map(dest => dest.DepositAmount, src => CalculateDeposit(src))
            .Map(dest => dest.RemainingAmount, src => CalculateRemaining(src))
            .Map(
                dest => dest.IsInventoryLocked,
                src => src.StatusId != null &&
                    src.StatusId != "pending" &&
                    src.StatusId != "waiting_deposit" &&
                    src.StatusId != "waiting_installment");
        config.NewConfig<Output, MyOrderResponse>()
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.OutputInfos, src => src.OutputInfos)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.ProvinceId, src => src.ProvinceId)
            .Map(dest => dest.WardCode, src => src.WardCode)
            .Map(dest => dest.Total, src => CalculateTotal(src))
            .Map(dest => dest.DepositAmount, src => CalculateDeposit(src))
            .Map(dest => dest.RemainingAmount, src => CalculateRemaining(src));
        config.NewConfig<OutputInfo, MyOrderItemResponse>()
            .Map(dest => dest.ProductName, src => MapProductName(src))
            .Map(dest => dest.VariantName, src => src.ProductVariant != null ? src.ProductVariant.VariantName : null)
            .Map(
                dest => dest.ColorName,
                src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorName : null)
            .Map(
                dest => dest.ColorCode,
                src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorCode : null)
            .Map(dest => dest.Count, src => src.Count)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.CoverImageUrl, src => MapCoverImageUrl(src));
        config.NewConfig<OutputInfo, OutputInfoResponse>()
            .Map(dest => dest.ProductVariantId, src => src.ProductVariantId)
            .Map(dest => dest.ProductVariantColorId, src => src.ProductVariantColorId)
            .Map(dest => dest.ProductName, src => MapProductName(src))
            .Map(dest => dest.VariantName, src => src.ProductVariant != null ? src.ProductVariant.VariantName : null)
            .Map(
                dest => dest.ColorName,
                src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorName : null)
            .Map(
                dest => dest.ColorCode,
                src => src.ProductVariantColor != null ? src.ProductVariantColor.ColorCode : null)
            .Map(dest => dest.CoverImageUrl, src => MapCoverImageUrl(src))
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

    private static decimal CalculateSubtotal(Output src)
    {
        return src.OutputInfos?.Sum(oi => (oi.Count ?? 0) * (oi.Price ?? 0)) ?? 0;
    }

    private static decimal CalculateShippingFee(Output src)
    {
        return src.ShippingFee ?? 0;
    }

    private static decimal? CalculateDeposit(Output src)
    {
        if (src.DepositRatio == null || src.DepositRatio == 0)
            return null;
        var subtotal = CalculateSubtotal(src);
        return subtotal * (src.DepositRatio.Value / 100m);
    }

    private static decimal? CalculateRemaining(Output src)
    {
        var total = CalculateTotal(src);
        var deposit = CalculateDeposit(src) ?? 0;
        return total - deposit;
    }

    private static string? MapProductName(OutputInfo src)
    {
        return src.ProductVariant?.Product?.Name;
    }

    private static string? MapCoverImageUrl(OutputInfo src)
    {
        if (src.ProductVariantColor != null && !string.IsNullOrWhiteSpace(src.ProductVariantColor.CoverImageUrl))
        {
            return src.ProductVariantColor.CoverImageUrl;
        }
        var variant = src.ProductVariant;
        if (variant == null)
            return null;
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

