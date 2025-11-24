using Application.ApiContracts.File;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Application.Interfaces.Services;
using Mapster;
using MediatR;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.Files.Commands.UploadFile;

public sealed class UploadImageCommandHandler(
    IFileStorageService fileStorageService,
    IMediaFileInsertRepository insertRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UploadImageCommand, MediaFileResponse>
{
    public async Task<MediaFileResponse> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            throw new ArgumentException("File is required");
        }

        // SaveFileAsync will validate and compress the image to WebP
        var (storagePath, fileExtension) = await fileStorageService.SaveFileAsync(request.File, cancellationToken).ConfigureAwait(false);

        // Get actual file size after compression
        var compressedFileResult = await fileStorageService.GetFileAsync(storagePath, cancellationToken).ConfigureAwait(false);
        var actualFileSize = compressedFileResult?.FileBytes.Length ?? 0;

        var mediaFile = new MediaFileEntity
        {
            StorageType = "local",
            StoragePath = storagePath,
            OriginalFileName = request.File.FileName,
            ContentType = "image/webp", // Always WebP after compression
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
