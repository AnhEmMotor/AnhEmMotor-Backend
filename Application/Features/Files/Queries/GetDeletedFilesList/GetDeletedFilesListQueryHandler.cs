using Application.ApiContracts.File;
using Application.Interfaces.Repositories.MediaFile;
using Application.Sieve;
using Domain.Enums;
using Domain.Shared;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace Application.Features.Files.Queries.GetDeletedFilesList;

public sealed class GetDeletedFilesListQueryHandler(
    IMediaFileReadRepository repository,
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService,
    ISieveProcessor sieveProcessor)
    : IRequestHandler<GetDeletedFilesListQuery, PagedResult<MediaFileResponse>>
{
    public async Task<PagedResult<MediaFileResponse>> Handle(GetDeletedFilesListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);
        SieveHelper.ApplyDefaultSorting(request.SieveModel, DataFetchMode.DeletedOnly);

        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);
        var files = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = await sieveProcessor.Apply(request.SieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

        var responses = files.Adapt<List<MediaFileResponse>>();
        foreach (var response in responses)
        {
            if (!string.IsNullOrEmpty(response.StoragePath))
            {
                response.PublicUrl = fileStorageService.GetPublicUrl(response.StoragePath);
            }
        }

        return new PagedResult<MediaFileResponse>(responses, totalCount, request.SieveModel.Page!.Value, request.SieveModel.PageSize!.Value);
    }
}
