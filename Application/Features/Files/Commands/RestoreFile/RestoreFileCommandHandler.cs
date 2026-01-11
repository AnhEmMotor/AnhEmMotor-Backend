using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Files.Commands.RestoreFile;

public sealed class RestoreFileCommandHandler(
    IMediaFileReadRepository readRepository,
    IMediaFileUpdateRepository updateRepository,
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreFileCommand, Result<MediaFileResponse?>>
{
    public async Task<Result<MediaFileResponse?>> Handle(
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
            return Error.NotFound();
        }

        updateRepository.Restore(mediaFile);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = mediaFile.Adapt<MediaFileResponse>();
        response.PublicUrl = fileStorageService.GetPublicUrl(mediaFile.StoragePath!);

        return response;
    }
}
