using Application.ApiContracts.PlateDossier.Responses;
using Domain.Entities;
using Mapster;

namespace Application.Features.PlateDossiers.Mappings
{
    public class PlateDossierMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PlateDossier, PlateDossierResponse>();
        }
    }
}
