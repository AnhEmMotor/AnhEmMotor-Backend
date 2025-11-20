using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.File;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.DeleteFile;

public sealed class DeleteFileCommandHandler(IMediaFileSelectRepository mediaFileSelectRepository, IMediaFileDeleteRepository mediaFileDeleteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteFileCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.FileName))
        {
            return new ErrorResponse { Errors = [new ErrorDetail { Message = "File name is required." }] };
        }

        try
        {
            var media = await mediaFileSelectRepository.GetByStoredFileNameAsync(request.FileName, cancellationToken, includeDeleted: false).ConfigureAwait(false);

            if (media is null)
            {
                var existed = await mediaFileSelectRepository.GetByStoredFileNameAsync(request.FileName, cancellationToken, includeDeleted: true).ConfigureAwait(false);
                if (existed is not null)
                {
                    return new ErrorResponse { Errors = [new ErrorDetail { Message = "File already deleted." }] };
                }

                return new ErrorResponse { Errors = [new ErrorDetail { Message = "File not found in database." }] };
            }

            mediaFileDeleteRepository.Delete(media);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }
        catch (Exception ex)
        {
            return new ErrorResponse { Errors = [new ErrorDetail { Message = $"Delete failed: {ex.Message}" }] };
        }
    }
}
