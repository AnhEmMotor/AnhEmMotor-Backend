using Application.ApiContracts.File.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using MediatR;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.Files.Queries.GetFilesList;

public sealed class GetFilesListQueryHandler(
    IMediaFileReadRepository repository,
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService,
    ISievePaginator paginator) : IRequestHandler<GetFilesListQuery, Domain.Primitives.PagedResult<MediaFileResponse>>
{
    public async Task<Domain.Primitives.PagedResult<MediaFileResponse>> Handle(
        GetFilesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        var result = await paginator.ApplyAsync<MediaFileEntity, MediaFileResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

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