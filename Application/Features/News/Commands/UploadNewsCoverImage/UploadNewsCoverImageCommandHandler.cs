using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.MediaFile.MediaFile;
using MediatR;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.News.Commands.UploadNewsCoverImage;

public class UploadNewsCoverImageCommandHandler(
    IFileReadService fileReadService,
    IFileInsertService fileInsertService,
    IMediaFileInsertRepository mediaFileInsertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadNewsCoverImageCommand, Result<UploadNewsCoverImageResponse>>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    public async Task<Result<UploadNewsCoverImageResponse>> Handle(
        UploadNewsCoverImageCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return Result<UploadNewsCoverImageResponse>.Failure("Filename is required");
        }
        if (request.FileStream == Stream.Null || !request.FileStream.CanRead || request.FileStream.Length == 0)
        {
            return Result<UploadNewsCoverImageResponse>.Failure("File is empty or required");
        }
        if (request.FileStream.Length > MaxFileSize)
        {
            return Result<UploadNewsCoverImageResponse>.Failure("File size exceeds 10MB limit");
        }
        var result = await fileInsertService.SaveFileAsync(request.FileStream, cancellationToken, "articles/covers")
            .ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result<UploadNewsCoverImageResponse>.Failure(result.Errors);
        }
        var savedFile = result.Value;
        var mediaFile = new MediaFileEntity
        {
            StorageType = "local",
            StoragePath = savedFile.StoragePath,
            OriginalFileName = request.FileName,
            ContentType = "image/webp",
            FileExtension = savedFile.Extension,
            FileSize = savedFile.Size
        };
        mediaFileInsertRepository.Add(mediaFile);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var publicUrl = fileReadService.GetPublicUrl(savedFile.StoragePath);
        return Result<UploadNewsCoverImageResponse>.Success(new UploadNewsCoverImageResponse { Url = publicUrl });
    }
}
