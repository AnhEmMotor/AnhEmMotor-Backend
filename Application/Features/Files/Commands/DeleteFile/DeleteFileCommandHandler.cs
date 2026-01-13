using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;

using Domain.Constants;
using MediatR;

namespace Application.Features.Files.Commands.DeleteFile;

public sealed class DeleteFileCommandHandler(
    IMediaFileReadRepository readRepository,
    IMediaFileDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork,
    Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService) : IRequestHandler<DeleteFileCommand, Result>
{
    public async Task<Result> Handle(
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
            return Result.Failure(Error.NotFound("File not found."));
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

        return Result.Success();
    }
}
