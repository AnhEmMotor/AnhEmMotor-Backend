using Application.ApiContracts.File;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Domain.Enums;
using Domain.Shared;
using MediatR;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.Files.Queries.GetDeletedFilesList;

public sealed class GetDeletedFilesListQueryHandler(
    IMediaFileReadRepository repository,
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService,
    ICustomSievePaginator paginator) : IRequestHandler<GetDeletedFilesListQuery, PagedResult<MediaFileResponse>>
{
    public async Task<PagedResult<MediaFileResponse>> Handle(
        GetDeletedFilesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        var result = await paginator.ApplyAsync<MediaFileEntity, MediaFileResponse>(
            query,
            request.SieveModel,
            DataFetchMode.DeletedOnly,
            cancellationToken);

        if(result.Items == null || result.Items.Count == 0)
        {
            return result;
        }

        foreach(var item in result.Items)
        {
            if(!string.IsNullOrEmpty(item.StoragePath))
            {
                item.PublicUrl = fileStorageService.GetPublicUrl(item.StoragePath);
            }
        }

        return result;
    }
}