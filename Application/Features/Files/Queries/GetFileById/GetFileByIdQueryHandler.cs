using Application.ApiContracts.File;
using Application.Interfaces.Repositories.MediaFile;
using Application.Interfaces.Services;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Files.Queries.GetFileById;

public sealed class GetFileByIdQueryHandler(
    IMediaFileReadRepository repository,
    IFileStorageService fileStorageService)
    : IRequestHandler<GetFileByIdQuery, (MediaFileResponse? Data, ErrorResponse? Error)>
{
    public async Task<(MediaFileResponse? Data, ErrorResponse? Error)> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        var file = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (file == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"File with Id {request.Id} not found." }]
            });
        }

        var response = file.Adapt<MediaFileResponse>();
        if (!string.IsNullOrEmpty(file.StoragePath))
        {
            response.PublicUrl = fileStorageService.GetPublicUrl(file.StoragePath);
        }

        return (response, null);
    }
}
