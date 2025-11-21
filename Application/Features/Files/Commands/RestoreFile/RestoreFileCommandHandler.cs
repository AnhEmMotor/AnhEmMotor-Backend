using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.File;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreFile;

public sealed class RestoreFileCommandHandler(
    IMediaFileSelectRepository selectRepository,
    IMediaFileRestoreRepository restoreRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreFileCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(RestoreFileCommand request, CancellationToken cancellationToken)
    {
        var mediaFile = await selectRepository.GetByStoredFileNameAsync(request.FileName, cancellationToken, includeDeleted: true).ConfigureAwait(false);
        
        if (mediaFile == null)
        {
            return new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"File '{request.FileName}' không tồn tại trong hệ thống." }]
            };
        }

        restoreRepository.Restore(mediaFile);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
