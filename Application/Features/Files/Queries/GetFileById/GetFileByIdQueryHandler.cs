using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.MediaFile.MediaFile;
using Mapster;
using MediatR;

namespace Application.Features.Files.Queries.GetFileById;

public sealed class GetFileByIdQueryHandler(IMediaFileReadRepository repository, IFileReadService fileReadService) : IRequestHandler<GetFileByIdQuery, Result<MediaFileResponse?>>
{
    public async Task<Result<MediaFileResponse?>> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        var file = await repository.GetByIdAsync(request.Id!.Value, cancellationToken).ConfigureAwait(false);
        if (file == null)
        {
            return Error.NotFound($"File with Id {request.Id} not found.");
        }
        var response = file.Adapt<MediaFileResponse>();
        if (!string.IsNullOrEmpty(file.StoragePath))
        {
            response.PublicUrl = fileReadService.GetPublicUrl(file.StoragePath);
        }
        return response;
    }
}

