using Mapster;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.Files.Mappings;

public sealed class MediaFileMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    { config.NewConfig<MediaFileEntity, ApiContracts.File.Responses.MediaFileResponse>(); }
}
