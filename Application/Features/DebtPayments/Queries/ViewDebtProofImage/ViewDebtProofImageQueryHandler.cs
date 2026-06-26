using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.MediaFile.MediaFile;
using Domain.Entities;
using MediatR;
using System.IO;

namespace Application.Features.DebtPayments.Queries.ViewDebtProofImage;

public class ViewDebtProofImageQueryHandler(
    IMediaFileReadRepository mediaFileReadRepository,
    IFileReadService fileReadService,
    Application.Interfaces.Repositories.SupplierDebt.ISupplierDebtReadRepository supplierDebtReadRepository) : IRequestHandler<ViewDebtProofImageQuery, Result<(Stream Content, string ContentType)>>
{
    public async Task<Result<(Stream Content, string ContentType)>> Handle(ViewDebtProofImageQuery request, CancellationToken cancellationToken)
    {
        var mediaFile = await mediaFileReadRepository.GetByIdAsync(request.MediaFileId, cancellationToken);
        if (mediaFile == null)
        {
            return Result<(Stream Content, string ContentType)>.Failure("Không tìm thấy ảnh.");
        }

        // Verify that this MediaFile is actually a Debt Proof
        var isDebtProof = await supplierDebtReadRepository.IsDebtProofImageAsync(request.MediaFileId, cancellationToken);
            
        if (!isDebtProof)
        {
            return Result<(Stream Content, string ContentType)>.Failure("Ảnh không hợp lệ hoặc bạn không có quyền xem.");
        }

        var fileResult = await fileReadService.GetFileAsync(mediaFile.StoragePath, cancellationToken);
        if (fileResult == null)
        {
            return Result<(Stream Content, string ContentType)>.Failure("Không tìm thấy file trên server.");
        }

        var stream = new MemoryStream(fileResult.Value.FileBytes);
        var contentType = string.IsNullOrEmpty(fileResult.Value.ContentType) ? "application/octet-stream" : fileResult.Value.ContentType;
        return Result<(Stream Content, string ContentType)>.Success((stream, contentType));
    }
}
