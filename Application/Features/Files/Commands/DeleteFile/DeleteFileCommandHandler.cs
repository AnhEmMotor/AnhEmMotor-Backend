using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Domain.Common.Models;
using Domain.Constants;
using MediatR;

namespace Application.Features.Files.Commands.DeleteFile;

public sealed class DeleteFileCommandHandler(
    IMediaFileReadRepository readRepository,
    IMediaFileDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork,
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService) : IRequestHandler<DeleteFileCommand, Common.Models.ErrorResponse?>
{
    public async Task<Common.Models.ErrorResponse?> Handle(
        DeleteFileCommand request,
        CancellationToken cancellationToken)
    {
        var mediaFile = await readRepository.GetByStoragePathAsync(
            request.StoragePath,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if(mediaFile is null)
        {
            return new Common.Models.ErrorResponse
            {
                Errors = [ new Common.Models.ErrorDetail { Message = "File not found." } ]
            };
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
