using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.LocalFile;
using Application.Interfaces.Repositories.MediaFile;
using Mapster;
using MediatR;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.Files.Commands.UploadImage;

public sealed class UploadImageCommandHandler(
    IFileStorageService fileStorageService,
    IMediaFileInsertRepository insertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadImageCommand, Result<MediaFileResponse>>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    public async Task<Result<MediaFileResponse>> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return Result<MediaFileResponse>.Failure("Filename is required");
        }

        if (request.FileContent == null || request.FileContent.Length == 0)
        {
            return Result<MediaFileResponse>.Failure("File is empty or required");
        }

        if (request.FileContent.Length > MaxFileSize)
        {
            return Result<MediaFileResponse>.Failure("File size exceeds 10MB limit");
        }

        var saveResult = await fileStorageService.SaveFileAsync(request.FileContent, cancellationToken)
            .ConfigureAwait(false);

        if(saveResult.IsFailure)
        {
            return Result<MediaFileResponse>.Failure(saveResult.Error ?? Error.Failure("Unknown upload error"));
        }

        var savedFile = saveResult.Value;

        var mediaFile = new MediaFileEntity
        {
            StorageType = "local",
            StoragePath = savedFile.StoragePath,
            OriginalFileName = request.FileName,
            ContentType = "image/webp",
            FileExtension = savedFile.Extension,
            FileSize = savedFile.Size
        };

        insertRepository.Add(mediaFile);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = mediaFile.Adapt<MediaFileResponse>();
        response.PublicUrl = fileStorageService.GetPublicUrl(savedFile.StoragePath);

        return response;
    }
}