using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.MediaFile.MediaFile;
using Application.Interfaces.Repositories.SupplierDebt;
using MediatR;

namespace Application.Features.DebtPayments.Queries.ViewDebtProofImage;

public class ViewDebtProofImageQueryHandler(
    IMediaFileReadRepository mediaFileReadRepository,
    IFileReadService fileReadService,
    ISupplierDebtReadRepository supplierDebtReadRepository) : IRequestHandler<ViewDebtProofImageQuery, Result<(Stream Content, string ContentType)>>
{
    public async Task<Result<(Stream Content, string ContentType)>> Handle(
        ViewDebtProofImageQuery request,
        CancellationToken cancellationToken)
    {
        var mediaFile = await mediaFileReadRepository.GetByIdAsync(request.MediaFileId, cancellationToken);
        if (mediaFile == null)
        {
            return Result<(Stream Content, string ContentType)>.Failure("Không tìm thấy ảnh.");
        }
        var isDebtProof = await supplierDebtReadRepository.IsDebtProofImageAsync(request.MediaFileId, cancellationToken);
        if (!isDebtProof)
        {
            return Result<(Stream Content, string ContentType)>.Failure("Ảnh không hợp lệ hoặc bạn không có quyền xem.");
        }
        if (string.IsNullOrEmpty(mediaFile.StoragePath))
        {
            return Result<(Stream Content, string ContentType)>.Failure("Đường dẫn file không hợp lệ.");
        }
        var fileResult = await fileReadService.GetFileAsync(mediaFile.StoragePath, cancellationToken);
        if (fileResult == null)
        {
            return Result<(Stream Content, string ContentType)>.Failure("Không tìm thấy file trên server.");
        }
        var stream = new MemoryStream(fileResult.Value.FileBytes);
        var contentType = string.IsNullOrEmpty(fileResult.Value.ContentType)
            ? "application/octet-stream"
            : fileResult.Value.ContentType;
        return Result<(Stream Content, string ContentType)>.Success((stream, contentType));
    }
}
