using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.File;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreMultipleFiles;

public sealed class RestoreMultipleFilesCommandHandler(
    IMediaFileSelectRepository selectRepository,
    IMediaFileRestoreRepository restoreRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreMultipleFilesCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(RestoreMultipleFilesCommand request, CancellationToken cancellationToken)
    {
        var mediaFiles = await selectRepository.GetByStoredFileNamesAsync(request.FileNames!, cancellationToken, includeDeleted: true).ConfigureAwait(false);
        
        if (mediaFiles == null || mediaFiles.Count == 0)
        {
            return new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = "Không tìm thấy file nào trong danh sách." }]
            };
        }

        if (mediaFiles.Count != request.FileNames.Count)
        {
            var foundFileNames = mediaFiles.Select(f => f.StoredFileName).ToHashSet();
            var missingFileNames = request.FileNames.Where(fn => !foundFileNames.Contains(fn)).ToList();
            
            return new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Không thể khôi phục vì một số file không tồn tại: {string.Join(", ", missingFileNames)}" }]
            };
        }

        restoreRepository.Restores(mediaFiles);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
