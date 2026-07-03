using Application.ApiContracts.WarrantyClaim.Responses;
using Domain.Entities;
using Mapster;
using System;

namespace Application.Features.WarrantyClaims.Mappings
{
    public class WarrantyClaimMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<WarrantyClaim, WarrantyClaimResponse>()
                .Map(dest => dest.VehiclePlate, src => src.Vehicle != null ? src.Vehicle.LicensePlate : null)
                .Map(dest => dest.CustomerName, src => src.Vehicle != null && src.Vehicle.User != null ? src.Vehicle.User.FullName : null)
                .Map(dest => dest.CustomerPhone, src => src.Vehicle != null && src.Vehicle.User != null ? src.Vehicle.User.PhoneNumber : null)
                .Map(dest => dest.StatusText, src => src.Status.ToString());

            config.NewConfig<WarrantyClaim, WarrantyClaimDetailResponse>()
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.CustomerName, src => src.Vehicle != null && src.Vehicle.User != null ? src.Vehicle.User.FullName : null)
                .Map(dest => dest.CustomerPhone, src => src.Vehicle != null && src.Vehicle.User != null ? src.Vehicle.User.PhoneNumber : null)
                .Map(dest => dest.VehicleVin, src => src.Vehicle != null ? src.Vehicle.VinNumber : null)
                .Map(dest => dest.VehiclePlate, src => src.Vehicle != null ? src.Vehicle.LicensePlate : null)
                .Map(dest => dest.VehicleColor, src => src.Vehicle != null && src.Vehicle.ProductVariantColor != null ? src.Vehicle.ProductVariantColor.ColorName : null)
                .Map(dest => dest.VehicleYear, src => src.Vehicle != null && src.Vehicle.ProductVariant != null ? src.Vehicle.ProductVariant.VariantName : null)
                .Map(dest => dest.MediaUrls, src => string.IsNullOrEmpty(src.MediaUrls) ? new System.Collections.Generic.List<string>() : new System.Collections.Generic.List<string>(src.MediaUrls.Split(',', StringSplitOptions.RemoveEmptyEntries)))
                .Map(dest => dest.WarrantyRemaining, src => CalculateRemainingWarranty(src.Vehicle));

            config.NewConfig<WarrantyClaimPart, WarrantyClaimPartResponse>()
                .Map(dest => dest.StatusText, src => src.Status.ToString());
        }

        private static string CalculateRemainingWarranty(Vehicle? vehicle)
        {
            if (vehicle == null) return "N/A";
            var limitMonths = 36; // Default to 36 months if parsing fails
            if (vehicle.Product != null && int.TryParse(vehicle.Product.WarrantyPeriod, out var pMonths))
            {
                limitMonths = pMonths;
            }
            var expiryDate = vehicle.PurchaseDate.AddMonths(limitMonths);
            var remaining = expiryDate - DateTimeOffset.UtcNow;
            if (remaining.TotalDays <= 0)
            {
                return "Hết hạn";
            }
            var remainingMonths = (int)Math.Ceiling(remaining.TotalDays / 30.0);
            return $"Còn {remainingMonths} tháng";
        }
    }
}
