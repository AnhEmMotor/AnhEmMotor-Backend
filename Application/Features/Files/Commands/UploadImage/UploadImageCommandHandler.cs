using Application.ApiContracts.File;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Mapster;
using MediatR;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.Files.Commands.UploadFile;

public sealed class UploadImageCommandHandler(
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService,
    IMediaFileInsertRepository insertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadImageCommand, MediaFileResponse>
{
    public async Task<MediaFileResponse> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        if(request.File == null || request.File.Length == 0)
        {
            throw new ArgumentException("File is required");
        }

        var (storagePath, fileExtension) = await fileStorageService.SaveFileAsync(request.File, cancellationToken)
            .ConfigureAwait(false);

        var compressedFileResult = await fileStorageService.GetFileAsync(storagePath, cancellationToken)
            .ConfigureAwait(false);
        var actualFileSize = compressedFileResult?.FileBytes.Length ?? 0;

        var mediaFile = new MediaFileEntity
        {
            StorageType = "local",
            StoragePath = storagePath,
            OriginalFileName = request.File.FileName,
            ContentType = "image/webp",
            FileExtension = fileExtension,
            FileSize = actualFileSize
        };

        insertRepository.Add(mediaFile);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = mediaFile.Adapt<MediaFileResponse>();
        response.PublicUrl = fileStorageService.GetPublicUrl(storagePath);

        return response;
    }
}
