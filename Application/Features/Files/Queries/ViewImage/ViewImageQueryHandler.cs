
using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Queries.ViewImage;

public sealed class ViewImageQueryHandler(Interfaces.Repositories.LocalFile.IFileStorageService fileStorageService) : IRequestHandler<ViewImageQuery, Result<(Stream FileStream, string ContentType)?>>
{
    public async Task<Result<(Stream FileStream, string ContentType)?>> Handle(
        ViewImageQuery request,
        CancellationToken cancellationToken)
    {
        var fileResult = await fileStorageService.GetFileAsync(request.StoragePath, cancellationToken)
            .ConfigureAwait(false);
        if(fileResult == null)
        {
            return Error.NotFound("Image not found.");
        }

        var (fileBytes, _) = fileResult.Value;

        using var inputStream = new MemoryStream(fileBytes);
        var processedStream = await fileStorageService.ReadImageAsync(inputStream, request.Width, cancellationToken)
            .ConfigureAwait(false);

        return (processedStream, "image/webp");
    }
}
