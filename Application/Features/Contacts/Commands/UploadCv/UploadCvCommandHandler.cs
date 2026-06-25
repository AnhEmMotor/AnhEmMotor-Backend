using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using MediatR;

namespace Application.Features.Contacts.Commands.UploadCv
{
    public sealed class UploadCvCommandHandler(IFileInsertService fileInsertService) : IRequestHandler<UploadCvCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(UploadCvCommand request, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(request.FileName).ToLower();
            var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".png", ".jpg", ".jpeg", ".webp" };
            if (!allowedExtensions.Contains(extension))
            {
                return Result<string>.Failure(
                    "Định dạng tệp không được hỗ trợ. Chỉ cho phép tải lên tệp PDF, Word (doc, docx) và hình ảnh.");
            }
            var uploadResult = await fileInsertService.SaveFileAsIsAsync(
                request.FileContent,
                request.FileName,
                cancellationToken,
                "cv")
                .ConfigureAwait(false);
            if (uploadResult.IsFailure)
            {
                return Result<string>.Failure(uploadResult.Error?.Message ?? "Không thể tải lên file CV.");
            }
            return Result<string>.Success(uploadResult.Value.StoragePath);
        }
    }
}
