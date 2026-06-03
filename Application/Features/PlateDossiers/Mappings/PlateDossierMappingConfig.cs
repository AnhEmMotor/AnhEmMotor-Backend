using Application.ApiContracts.PlateDossier.Responses;
using Domain.Entities;
using Mapster;
using System.Linq;

namespace Application.Features.PlateDossiers.Mappings
{
    public class PlateDossierMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PlateDossier, PlateDossierResponse>()
                .Map(dest => dest.CustomerName, src => src.Output.CustomerName)
                .Map(dest => dest.CustomerPhone, src => src.Output.CustomerPhone)
                .Map(dest => dest.VehicleName, src => src.Output.OutputInfos
                    .Select(oi => oi.ProductVariant != null ? oi.ProductVariant.VariantName : null)
                    .FirstOrDefault(name => name != null));
        }
    }
}
