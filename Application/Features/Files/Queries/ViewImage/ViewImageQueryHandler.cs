
using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using MediatR;

namespace Application.Features.Files.Queries.ViewImage;

public sealed class ViewImageQueryHandler(IFileReadService fileReadService) : IRequestHandler<ViewImageQuery, Result<(Stream FileStream, string ContentType)?>>
{
    public async Task<Result<(Stream FileStream, string ContentType)?>> Handle(
        ViewImageQuery request,
        CancellationToken cancellationToken)
    {
        var fileResult = await fileReadService.GetFileAsync(request.StoragePath, cancellationToken)
            .ConfigureAwait(false);
        if (fileResult == null)
        {
            return Error.NotFound("Image not found.");
        }
        var (fileBytes, contentType) = fileResult.Value;
        if (contentType == "application/pdf" ||
            contentType.Contains("document") ||
            contentType.Contains("msword") ||
            contentType == "application/octet-stream")
        {
            var rawStream = new MemoryStream(fileBytes);
            return (rawStream, contentType);
        }

        using var inputStream = new MemoryStream(fileBytes);
        var processedStream = await fileReadService.ReadImageAsync(inputStream, request.Width, cancellationToken)
            .ConfigureAwait(false);
        return (processedStream, "image/webp");
    }
}
