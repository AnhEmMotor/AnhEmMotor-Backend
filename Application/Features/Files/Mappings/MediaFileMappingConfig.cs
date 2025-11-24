using Application.ApiContracts.File;
using Application.Features.Files.Commands.DeleteManyFiles;
using Application.Features.Files.Commands.RestoreManyFiles;
using Mapster;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.Files.Mappings;

public sealed class MediaFileMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MediaFileEntity, MediaFileResponse>();

        config.NewConfig<DeleteManyMediaFilesRequest, DeleteManyFilesCommand>();

        config.NewConfig<RestoreManyMediaFilesRequest, RestoreManyFilesCommand>();
    }
}
