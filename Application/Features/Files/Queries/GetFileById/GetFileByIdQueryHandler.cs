using Application.Interfaces.Repositories.MediaFile;
using Mapster;
using MediatR;

namespace Application.Features.Files.Queries.GetFileById;

public sealed class GetFileByIdQueryHandler(
    IMediaFileReadRepository repository,
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService) : IRequestHandler<GetFileByIdQuery, (ApiContracts.File.Responses.MediaFileResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public async Task<(ApiContracts.File.Responses.MediaFileResponse? Data, Common.Models.ErrorResponse? Error)> Handle(
        GetFileByIdQuery request,
        CancellationToken cancellationToken)
    {
        var file = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(file == null)
        {
            return (null, new Common.Models.ErrorResponse
            {
                Errors = [ new Common.Models.ErrorDetail { Message = $"File with Id {request.Id} not found." } ]
            });
        }

        var response = file.Adapt<ApiContracts.File.Responses.MediaFileResponse>();
        if(!string.IsNullOrEmpty(file.StoragePath))
        {
            response.PublicUrl = fileStorageService.GetPublicUrl(file.StoragePath);
        }

        return (response, null);
    }
}
