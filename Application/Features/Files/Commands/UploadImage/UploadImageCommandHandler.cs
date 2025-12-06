using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Mapster;
using MediatR;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.Files.Commands.UploadImage;

public sealed class UploadImageCommandHandler(
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService,
    IMediaFileInsertRepository insertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadImageCommand, ApiContracts.File.Responses.MediaFileResponse>
{
    public async Task<ApiContracts.File.Responses.MediaFileResponse> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        if(request.FileContent == null || request.FileContent.Length == 0)
        {
            throw new ArgumentException("File is required");
        }

        var (storagePath, fileExtension) = await fileStorageService.SaveFileAsync(
            request.FileContent,
            cancellationToken)
            .ConfigureAwait(false);

        var compressedFileResult = await fileStorageService.GetFileAsync(storagePath, cancellationToken)
            .ConfigureAwait(false);
        var actualFileSize = compressedFileResult?.FileBytes.Length ?? 0;

        var mediaFile = new MediaFileEntity
        {
            StorageType = "local",
            StoragePath = storagePath,
            OriginalFileName = request.FileName,
            ContentType = "image/webp",
            FileExtension = fileExtension,
            FileSize = actualFileSize
        };

        insertRepository.Add(mediaFile);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = mediaFile.Adapt<ApiContracts.File.Responses.MediaFileResponse>();
        response.PublicUrl = fileStorageService.GetPublicUrl(storagePath);

        return response;
    }
}