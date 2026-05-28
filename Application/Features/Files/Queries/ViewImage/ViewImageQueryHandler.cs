
using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using MediatR;

namespace Application.Features.Files.Queries.ViewImage;

public sealed class ViewImageQueryHandler(IFileReadService fileReadService) : IRequestHandler<ViewImageQuery, Result<ViewImageResponse?>>
{
    public async Task<Result<ViewImageResponse?>> Handle(
        ViewImageQuery request,
        CancellationToken cancellationToken)
    {
        var fileResult = await fileReadService.GetFileAsync(request.StoragePath, cancellationToken)
            .ConfigureAwait(false);
        if (fileResult == null)
        {
            return Error.NotFound("Image not found.");
        }
        var (fileBytes, _) = fileResult.Value;
        using var inputStream = new MemoryStream(fileBytes);
        var processedStream = await fileReadService.ReadImageAsync(inputStream, request.Width, cancellationToken)
            .ConfigureAwait(false);
        return new ViewImageResponse { FileStream = processedStream, ContentType = "image/webp" };
    }
}
