using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.LocalFile;
using Application.Interfaces.Repositories.MediaFile;
using Mapster;
using MediatR;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.Files.Commands.UploadManyImage;

public sealed class UploadManyImageCommandHandler(
    IFileStorageService fileStorageService,
    IMediaFileInsertRepository insertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadManyImageCommand, Result<List<MediaFileResponse>>>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    public async Task<Result<List<MediaFileResponse>>> Handle(
        UploadManyImageCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Files == null || request.Files.Count == 0)
        {
            return Result<List<MediaFileResponse>>.Failure("No files to upload");
        }

        // --- PHASE 1: PRE-VALIDATION (Chặn ngay từ cửa) ---
        var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp" };

        foreach (var fileDto in request.Files)
        {
            // Check 1: Size
            if (fileDto.FileContent.Length > MaxFileSize)
            {
                return Result<List<MediaFileResponse>>.Failure($"File {fileDto.FileName} exceeds 10MB limit");
            }

            // Check 2: Extension (Logic bạn đang thiếu)
            var ext = Path.GetExtension(fileDto.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                return Result<List<MediaFileResponse>>.Failure($"File format '{ext}' is not supported in file {fileDto.FileName}");
            }
        }

        var mediaFiles = new List<MediaFileEntity>();

        foreach(var fileDto in request.Files)
        {
            if(fileDto.FileContent.Length > MaxFileSize)
            {
                return Result<List<MediaFileResponse>>.Failure($"File {fileDto.FileName} exceeds 10MB limit");
            }

            var saveResult = await fileStorageService.SaveFileAsync(fileDto.FileContent, cancellationToken)
                .ConfigureAwait(false);

            if(saveResult.IsFailure)
            {
                return Result<List<MediaFileResponse>>.Failure(
                    saveResult.Error ?? Error.Failure("Unknown upload error"));
            }

            var savedFile = saveResult.Value;

            mediaFiles.Add(
                new MediaFileEntity
                {
                    StorageType = "local",
                    StoragePath = savedFile.StoragePath,
                    OriginalFileName = fileDto.FileName,
                    ContentType = "image/webp",
                    FileExtension = savedFile.Extension,
                    FileSize = savedFile.Size
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