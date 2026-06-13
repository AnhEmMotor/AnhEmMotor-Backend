using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.MediaFile.MediaFile;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Files.Queries.GetFilesList;

public class GetFilesListQueryHandler(IMediaFileReadRepository repository, IFileReadService fileReadService) : IRequestHandler<GetFilesListQuery, Result<PagedResult<MediaFileResponse>>>
{
    public async Task<Result<PagedResult<MediaFileResponse>>> Handle(
        GetFilesListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<MediaFileResponse>(
            request.SieveModel!,
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
                item.PublicUrl = fileReadService.GetPublicUrl(item.StoragePath);
            }
        }
        return result;
    }
}
