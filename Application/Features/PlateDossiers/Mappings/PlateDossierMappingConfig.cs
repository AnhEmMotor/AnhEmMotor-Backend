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
                .Map(dest => dest.VehicleName, src => src.Output != null && src.Output.OutputInfos != null
                    ? src.Output.OutputInfos
                        .Where(oi => oi.ProductVariant != null && oi.ProductVariant.Product != null)
                        .Select(oi => oi.ProductVariant.Product!.Name)
                        .FirstOrDefault()
                    : null);
        }
    }
}
