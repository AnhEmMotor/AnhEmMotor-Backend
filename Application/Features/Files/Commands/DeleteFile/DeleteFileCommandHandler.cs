using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Domain.Constants;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.DeleteFile;

public sealed class DeleteFileCommandHandler(
    IMediaFileReadRepository readRepository,
    IMediaFileDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork,
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService) : IRequestHandler<DeleteFileCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var mediaFile = await readRepository.GetByStoragePathAsync(
            request.StoragePath,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if(mediaFile is null)
        {
            return new ErrorResponse { Errors = [ new ErrorDetail { Message = "File not found." } ] };
        }

        deleteRepository.Delete(mediaFile);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if(!string.IsNullOrWhiteSpace(mediaFile.StoragePath))
        {
            try
            {
                fileStorageService.DeleteFile(mediaFile.StoragePath);
            } catch
            {
            }
        }

        return null;
    }
}
