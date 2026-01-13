using Application.Features.Files.Commands.DeleteManyFiles;
using Application.Features.Files.Commands.RestoreManyFiles;
using Mapster;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Files.Mappings;

public sealed class MediaFileMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MediaFileEntity, ApiContracts.File.Responses.MediaFileResponse>();
    }
}
