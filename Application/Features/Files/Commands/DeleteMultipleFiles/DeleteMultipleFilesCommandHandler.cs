using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.File;
using Domain.Entities;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.DeleteMultipleFiles;

public sealed class DeleteMultipleFilesCommandHandler(IMediaFileSelectRepository mediaFileSelectRepository, IMediaFileDeleteRepository mediaFileDeleteRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteMultipleFilesCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteMultipleFilesCommand request, CancellationToken cancellationToken)
    {
        if (request.FileNames == null || request.FileNames.Count == 0)
        {
            return null;
        }

        var uniqueFileNames = request.FileNames.Distinct().ToList();
        var errorDetails = new List<ErrorDetail>();

        List<MediaFile>? allMediaFile = await mediaFileSelectRepository.GetByStoredFileNamesAsync(request.FileNames, cancellationToken, true).ConfigureAwait(false);
        List<MediaFile>? activeMediaFile = await mediaFileSelectRepository.GetByStoredFileNamesAsync(request.FileNames, cancellationToken, false).ConfigureAwait(false);

        var allMediaFileMap = allMediaFile!.ToDictionary(b => b.StoredFileName!);
        var activeMediaFileSet = activeMediaFile!.Select(b => b.StoredFileName!).ToHashSet();

        foreach (var fileName in uniqueFileNames)
        {
            if (!allMediaFileMap.ContainsKey(fileName!))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Message = $"File '{fileName}' not found."
                });
            }

            if (!activeMediaFileSet.Contains(fileName!))
            {
                errorDetails.Add(new ErrorDetail
                {
                    Field = "Id",
                    Message = $"File '{fileName}' has already been deleted"
                });
            }
        }

        if (errorDetails.Count > 0)
        {
            return new ErrorResponse { Errors = errorDetails };
        }

        if (uniqueFileNames.Count > 0)
        {
            mediaFileDeleteRepository.DeleteRange(allMediaFile!);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}
