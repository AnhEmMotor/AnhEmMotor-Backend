using Application.ApiContracts.File;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.File;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreMultipleFiles;

public sealed class RestoreMultipleFilesCommandHandler(
    IMediaFileSelectRepository selectRepository,
    IMediaFileRestoreRepository restoreRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreMultipleFilesCommand, (List<FileResponse>?, ErrorResponse?)>
{
    public async Task<(List<FileResponse>?, ErrorResponse?)> Handle(RestoreMultipleFilesCommand request, CancellationToken cancellationToken)
    {
        var mediaFiles = await selectRepository.GetByStoredFileNamesAsync(request.FileNames!, cancellationToken, includeDeleted: true).ConfigureAwait(false);
        
        if (mediaFiles == null || mediaFiles.Count == 0)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = "Không tìm thấy file nào trong danh sách." }]
            });
        }

        if (mediaFiles.Count != request.FileNames.Count)
        {
            var foundFileNames = mediaFiles.Select(f => f.StoredFileName).ToHashSet();
            var missingFileNames = request.FileNames.Where(fn => !foundFileNames.Contains(fn)).ToList();
            
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Không thể khôi phục vì một số file không tồn tại: {string.Join(", ", missingFileNames)}" }]
            });
        }

        // Check if ALL files are deleted (not active)
        var activeFiles = await selectRepository.GetByStoredFileNamesAsync(request.FileNames!, cancellationToken, includeDeleted: false).ConfigureAwait(false);
        if (activeFiles is not null && activeFiles.Count > 0)
        {
            var activeFileNames = activeFiles.Select(f => f.StoredFileName).ToList();
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Không thể khôi phục vì một số file đã được khôi phục trước đó: {string.Join(", ", activeFileNames)}" }]
            });
        }

        restoreRepository.Restores(mediaFiles);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var responses = mediaFiles.Select(f => new FileResponse
        {
            IsSuccess = true,
            FileName = f.OriginalFileName,
            Url = f.PublicUrl
        }).ToList();

        return (responses, null);
    }
}
