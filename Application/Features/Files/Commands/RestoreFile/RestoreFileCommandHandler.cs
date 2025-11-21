using Application.ApiContracts.File;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.File;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreFile;

public sealed class RestoreFileCommandHandler(
    IMediaFileSelectRepository selectRepository,
    IMediaFileRestoreRepository restoreRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreFileCommand, (FileResponse?, ErrorResponse?)>
{
    public async Task<(FileResponse?, ErrorResponse?)> Handle(RestoreFileCommand request, CancellationToken cancellationToken)
    {
        var mediaFile = await selectRepository.GetByStoredFileNameAsync(request.FileName, cancellationToken, includeDeleted: true).ConfigureAwait(false);
        
        if (mediaFile == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"File '{request.FileName}' không tồn tại trong hệ thống." }]
            });
        }

        // Check if file is already active (not deleted)
        var activeFile = await selectRepository.GetByStoredFileNameAsync(request.FileName, cancellationToken, includeDeleted: false).ConfigureAwait(false);
        if (activeFile is not null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"File '{request.FileName}' đã được khôi phục trước đó." }]
            });
        }

        restoreRepository.Restore(mediaFile);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new FileResponse
        {
            IsSuccess = true,
            FileName = mediaFile.OriginalFileName,
            Url = mediaFile.PublicUrl
        }, null);
    }
}
