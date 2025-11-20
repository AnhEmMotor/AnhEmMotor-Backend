using Application.ApiContracts.File;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Domain.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace Application.Features.Files.Commands.UploadFile;

public sealed class UploadFileCommandHandler(IFileRepository fileRepository, IMediaFileInsertRepository mediaFileInsertRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UploadFileCommand, (FileResponse? Data, ErrorResponse? Error)>
{
    private readonly string[] _PermittedExtensions = [".jpg", ".jpeg", ".png", ".gif"];
    private readonly Dictionary<string, string> _MimeTypeMappings = new()
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" }
    };

    public async Task<(FileResponse? Data, ErrorResponse? Error)> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var (mediaFile, responseData, error) = await ProcessAndSaveFileToDiskAsync(request.File, request.BaseUrl, cancellationToken).ConfigureAwait(false);

        if (error is not null || mediaFile is null || responseData is null)
        {
            return (null, error ?? new ErrorResponse { Errors = [new ErrorDetail { Message = "Unknown error occurred." }] });
        }

        try
        {
            mediaFileInsertRepository.Add(mediaFile);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Database save failed: {ex.Message}" }]
            });
        }

        return (responseData, null);
    }

    private async Task<(MediaFile? FileData, FileResponse? Response, ErrorResponse? Error)> ProcessAndSaveFileToDiskAsync(
        IFormFile file, string baseUrl, CancellationToken cancellationToken)
    {
        var (isValid, validationError) = await ValidateFileAsync(file, cancellationToken).ConfigureAwait(false);
        if (!isValid)
        {
            return (null, null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = validationError }]
            });
        }

        var originalFileName = file.FileName;
        var storedFileName = $"{Guid.NewGuid()}.webp";
        var relativePath = Path.Combine("uploads", storedFileName);

        long fileSize;
        try
        {
            using var image = await Image.LoadAsync(file.OpenReadStream(), cancellationToken).ConfigureAwait(false);
            using var outputStream = new MemoryStream();

            await image.SaveAsync(outputStream, new WebpEncoder { Quality = 75 }, cancellationToken).ConfigureAwait(false);
            fileSize = outputStream.Length;

            await fileRepository.SaveFileAsync(outputStream, relativePath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var errMsg = $"Image processing failed: {ex.Message}";
            return (null, null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = errMsg }]
            });
        }

        var publicUrl = $"{baseUrl}/uploads/{storedFileName}";

        var mediaFile = new MediaFile
        {
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            ContentType = "image/webp",
            PublicUrl = publicUrl,
            FileSize = fileSize,
        };

        var uploadResponse = new FileResponse
        {
            IsSuccess = true,
            FileName = storedFileName,
            Url = publicUrl
        };

        return (mediaFile, uploadResponse, null);
    }

    private async Task<(bool, string)> ValidateFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "File is empty.");
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return (false, "File size exceeds 10MB limit.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !_PermittedExtensions.Contains(ext))
        {
            return (false, "Invalid file type. Only JPG, PNG, GIF are allowed.");
        }

        if (!_MimeTypeMappings.ContainsValue(file.ContentType.ToLowerInvariant()))
        {
            return (false, "Invalid MIME type.");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var info = await Image.IdentifyAsync(stream, cancellationToken).ConfigureAwait(false);
            if (info == null)
            {
                return (false, "File is not a valid image.");
            }
        }
        catch
        {
            return (false, "File is corrupted or not a valid image.");
        }

        return (true, string.Empty);
    }
}
