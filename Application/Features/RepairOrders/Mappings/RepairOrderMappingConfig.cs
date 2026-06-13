using Application.ApiContracts.RepairOrder.Responses;
using Domain.Entities;
using Mapster;
using System.Linq;

namespace Application.Features.RepairOrders.Mappings
{
    public class RepairOrderMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RepairOrder, RepairOrderResponse>()
                .Map(dest => dest.LicensePlate, src => src.Vehicle != null ? src.Vehicle.LicensePlate : null)
                .Map(
                    dest => dest.TechnicianName,
                    src => src.Technician != null && src.Technician.User != null ? src.Technician.User.FullName : null)
                .Map(dest => dest.Details, src => src.Details);
            config.NewConfig<RepairOrderDetail, RepairOrderDetailResponse>()
                .Map(dest => dest.ServiceName, src => src.Service != null ? src.Service.Name : null)
                .Map(
                    dest => dest.VariantName,
                    src => src.ProductVariant != null ? src.ProductVariant.VariantName : null)
                .Map(dest => dest.ProductCode, src => src.ProductVariant != null ? src.ProductVariant.SKU : null);
        }
    }
}
