using Application.ApiContracts.File;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Mapster;
using MediatR;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.Files.Commands.UploadManyImage;

public sealed class UploadManyImageCommandHandler(
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService,
    IMediaFileInsertRepository insertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadManyImageCommand, List<MediaFileResponse>>
{
    public async Task<List<MediaFileResponse>> Handle(
        UploadManyImageCommand request,
        CancellationToken cancellationToken)
    {
        if(request.Files == null || request.Files.Count == 0)
        {
            return [];
        }

        var mediaFiles = new List<MediaFileEntity>();

        foreach(var fileDto in request.Files)
        {
            var (storagePath, fileExtension) = await fileStorageService.SaveFileAsync(
                fileDto.FileContent,
                cancellationToken)
                .ConfigureAwait(false);

            var compressedFileResult = await fileStorageService.GetFileAsync(storagePath, cancellationToken)
                .ConfigureAwait(false);
            var actualFileSize = compressedFileResult?.FileBytes.Length ?? 0;

            mediaFiles.Add(
                new MediaFileEntity
                {
                    StorageType = "local",
                    StoragePath = storagePath,
                    OriginalFileName = fileDto.FileName,
                    ContentType = "image/webp",
                    FileExtension = fileExtension,
                    FileSize = actualFileSize
                });
        }

        insertRepository.AddRange(mediaFiles);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var responses = mediaFiles.Adapt<List<MediaFileResponse>>();
        foreach(var response in responses)
        {
            response.PublicUrl = fileStorageService.GetPublicUrl(response.StoragePath!);
        }

        return responses;
    }
}