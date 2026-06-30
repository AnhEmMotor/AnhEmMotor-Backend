using Application.ApiContracts.Maintenance.Responses;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Application.Features.Maintenances.Mappings
{
    public class MaintenanceMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MaintenanceHistory, MaintenanceTicketResponse>()
                .Map(dest => dest.VehiclePlate, src => src.Vehicle != null ? src.Vehicle.LicensePlate : null)
                .Map(dest => dest.CustomerName, src => src.Vehicle != null && src.Vehicle.User != null ? src.Vehicle.User.FullName : null)
                .Map(dest => dest.CustomerPhone, src => src.Vehicle != null && src.Vehicle.User != null ? src.Vehicle.User.PhoneNumber : null);

            config.NewConfig<MaintenanceHistory, MaintenanceTicketDetailResponse>()
                .Map(dest => dest.CustomerName, src => src.Vehicle != null && src.Vehicle.User != null ? src.Vehicle.User.FullName : null)
                .Map(dest => dest.CustomerPhone, src => src.Vehicle != null && src.Vehicle.User != null ? src.Vehicle.User.PhoneNumber : null)
                .Map(dest => dest.CustomerAddress, src => src.Vehicle != null && src.Vehicle.Lead != null ? src.Vehicle.Lead.Address : null)
                .Map(dest => dest.VehicleVin, src => src.Vehicle != null ? src.Vehicle.VinNumber : null)
                .Map(dest => dest.VehiclePlate, src => src.Vehicle != null ? src.Vehicle.LicensePlate : null)
                .Map(dest => dest.VehicleColor, src => src.Vehicle != null && src.Vehicle.ProductVariantColor != null ? src.Vehicle.ProductVariantColor.ColorName : null)
                .Map(dest => dest.VehicleYear, src => src.Vehicle != null && src.Vehicle.ProductVariant != null ? src.Vehicle.ProductVariant.VariantName : null)
                .Map(dest => dest.TechnicianName, src => src.Technician != null && src.Technician.User != null ? src.Technician.User.FullName : null)
                .Map(dest => dest.Parts, src => ParseParts(src.PartsJson));
        }

        private static List<MaintenancePartDto> ParseParts(string? partsJson)
        {
            if (string.IsNullOrEmpty(partsJson)) return new List<MaintenancePartDto>();
            try
            {
                return JsonSerializer.Deserialize<List<MaintenancePartDto>>(partsJson) ?? new List<MaintenancePartDto>();
            }
            catch
            {
                return new List<MaintenancePartDto>();
            }
        }
    }
}
