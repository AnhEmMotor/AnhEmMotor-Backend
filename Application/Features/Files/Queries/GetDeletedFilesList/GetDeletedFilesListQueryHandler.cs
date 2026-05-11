using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.LocalFile;
using Application.Interfaces.Repositories.MediaFile;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Files.Queries.GetDeletedFilesList;

public sealed class GetDeletedFilesListQueryHandler(
    IMediaFileReadRepository repository,
    IFileStorageService fileStorageService) : IRequestHandler<GetDeletedFilesListQuery, Result<PagedResult<MediaFileResponse>>>
{
    public async Task<Result<PagedResult<MediaFileResponse>>> Handle(
        GetDeletedFilesListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<MediaFileResponse>(
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (result.Items == null || result.Items.Count == 0)
        {
            return result;
        }
        foreach (var item in result.Items)
        {
            if (!string.IsNullOrEmpty(item.StoragePath))
            {
                item.PublicUrl = fileStorageService.GetPublicUrl(item.StoragePath);
            }
        }
        return result;
    }
}