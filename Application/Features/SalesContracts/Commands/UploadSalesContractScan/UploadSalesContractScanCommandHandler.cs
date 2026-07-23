using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Constants;
using MediatR;

namespace Application.Features.SalesContracts.Commands.UploadSalesContractScan;

public sealed class UploadSalesContractScanCommandHandler(
    ISalesContractReadRepository readRepository,
    IFileInsertService fileInsertService,
    IFileReadService fileReadService,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadSalesContractScanCommand, Result<string>>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };

    public async Task<Result<string>> Handle(
        UploadSalesContractScanCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            return Result<string>.Failure("Tên file là bắt buộc.");
        if (request.FileContent is null || !request.FileContent.CanRead || request.FileContent.Length == 0)
            return Result<string>.Failure("File hợp đồng trống hoặc không hợp lệ.");
        if (request.FileContent.Length > MaxFileSize)
            return Result<string>.Failure("File hợp đồng không được vượt quá 10MB.");
        var extension = Path.GetExtension(request.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            return Result<string>.Failure("Chỉ hỗ trợ file PDF, JPG, JPEG hoặc PNG.");
        var contract = await readRepository.GetByIdAsync(request.ContractId, cancellationToken).ConfigureAwait(false);
        if (contract is null)
            return Result<string>.Failure("Không tìm thấy hợp đồng.");
        if (string.Equals(contract.Status, SalesContractStatus.Draft, StringComparison.Ordinal))
            return Result<string>.Failure("Hợp đồng phải được Admin duyệt trước khi tải bản quét đã ký.");
        if (string.Equals(contract.Status, SalesContractStatus.Fulfilled, StringComparison.Ordinal))
            return Result<string>.Failure("Hợp đồng đã hoàn tất nên không thể thay đổi bản quét.");
        if (!string.Equals(contract.Status, SalesContractStatus.Approved, StringComparison.Ordinal) &&
            !string.Equals(contract.Status, SalesContractStatus.Signed, StringComparison.Ordinal))
            return Result<string>.Failure("Trạng thái hợp đồng không cho phép tải bản quét đã ký.");
        var uploadResult = await fileInsertService.SaveFileAsIsAsync(
            request.FileContent,
            request.FileName,
            cancellationToken,
            $"sales-contracts/{request.ContractId}")
            .ConfigureAwait(false);
        if (uploadResult.IsFailure)
            return Result<string>.Failure(uploadResult.Error?.Message ?? "Không thể lưu file hợp đồng.");
        var scannedFileUrl = fileReadService.GetPublicUrl(uploadResult.Value.StoragePath);
        contract.ScannedFileUrl = scannedFileUrl;
        contract.Status = SalesContractStatus.Signed;
        contract.SignedDate ??= DateTimeOffset.UtcNow;
        contract.UpdatedAt = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<string>.Success(scannedFileUrl);
    }
}
