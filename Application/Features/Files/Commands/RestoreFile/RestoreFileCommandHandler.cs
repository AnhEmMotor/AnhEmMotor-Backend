using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Domain.Constants;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Files.Commands.RestoreFile;

public sealed class RestoreFileCommandHandler(
    IMediaFileReadRepository readRepository,
    IMediaFileUpdateRepository updateRepository,
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreFileCommand, (ApiContracts.File.Responses.MediaFileResponse?, ErrorResponse?)>
{
    public async Task<(ApiContracts.File.Responses.MediaFileResponse?, ErrorResponse?)> Handle(
        RestoreFileCommand request,
        CancellationToken cancellationToken)
    {
        var mediaFile = await readRepository.GetByStoragePathAsync(
            request.StoragePath,
            cancellationToken,
            DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);

        if(mediaFile is null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Message = "File not found or not deleted." } ]
            });
        }

        updateRepository.Restore(mediaFile);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = mediaFile.Adapt<ApiContracts.File.Responses.MediaFileResponse>();
        response.PublicUrl = fileStorageService.GetPublicUrl(mediaFile.StoragePath!);

        return (response, null);
    }
}
