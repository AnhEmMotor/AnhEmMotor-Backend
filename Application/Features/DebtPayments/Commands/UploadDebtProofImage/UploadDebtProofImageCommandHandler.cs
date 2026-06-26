using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.MediaFile.MediaFile;
using MediatR;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace Application.Features.DebtPayments.Commands.UploadDebtProofImage;

public class UploadDebtProofImageCommandHandler(
    IFileReadService fileReadService,
    IFileInsertService fileInsertService,
    IMediaFileInsertRepository insertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadDebtProofImageCommand, Result<UploadDebtProofImageResponse>>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    public async Task<Result<UploadDebtProofImageResponse>> Handle(
        UploadDebtProofImageCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return Result<UploadDebtProofImageResponse>.Failure("Filename is required");
        }
        if (request.FileContent == null || request.FileContent.Length == 0)
        {
            return Result<UploadDebtProofImageResponse>.Failure("File is empty or required");
        }
        if (request.FileContent.Length > MaxFileSize)
        {
            return Result<UploadDebtProofImageResponse>.Failure("File size exceeds 10MB limit");
        }

        var saveResult = await fileInsertService.SaveFileAsync(request.FileContent, cancellationToken, "debt-proofs")
            .ConfigureAwait(false);
            
        if (saveResult.IsFailure)
        {
            return Result<UploadDebtProofImageResponse>.Failure(saveResult.Error ?? Error.Failure("Unknown upload error"));
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
        
        var url = fileReadService.GetPublicUrl(savedFile.StoragePath);

        return Result<UploadDebtProofImageResponse>.Success(new UploadDebtProofImageResponse
        {
            MediaFileId = mediaFile.Id,
            Url = url
        });
    }
}
