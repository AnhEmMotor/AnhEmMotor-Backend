using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile.File;
using Application.Interfaces.Repositories.SupplierContract;
using MediatR;

namespace Application.Features.SupplierContracts.Commands.UploadSupplierContractFile;

public sealed class UploadSupplierContractFileCommandHandler(
    ISupplierContractReadRepository readRepository,
    IFileInsertService fileInsertService,
    IFileReadService fileReadService,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadSupplierContractFileCommand, Result<string>>
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };

    public async Task<Result<string>> Handle(
        UploadSupplierContractFileCommand request,
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
            return Result<string>.Failure("Chỉ hỗ trợ file PDF, Word, JPG, JPEG hoặc PNG.");

        var contract = await readRepository
            .GetByIdAsync(request.ContractId, cancellationToken)
            .ConfigureAwait(false);
        if (contract is null)
            return Result<string>.Failure("Không tìm thấy hợp đồng nhà cung cấp.");

        var uploadResult = await fileInsertService.SaveFileAsIsAsync(
            request.FileContent,
            request.FileName,
            cancellationToken,
            $"supplier-contracts/{request.ContractId}").ConfigureAwait(false);
        if (uploadResult.IsFailure)
            return Result<string>.Failure(uploadResult.Error?.Message ?? "Không thể lưu file hợp đồng.");

        var publicUrl = fileReadService.GetPublicUrl(uploadResult.Value.StoragePath);
        contract.ContractFilePath = publicUrl;
        contract.UpdatedAt = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<string>.Success(publicUrl);
    }
}
