
using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using MediatR;

namespace Application.Features.Files.Queries.ViewImage;

public class ViewImageQueryHandler(IFileReadService fileReadService) : IRequestHandler<ViewImageQuery, Result<ViewImageResponse?>>
{
    public async Task<Result<ViewImageResponse?>> Handle(ViewImageQuery request, CancellationToken cancellationToken)
    {
        var fileResult = await fileReadService.GetFileAsync(request.StoragePath, cancellationToken)
            .ConfigureAwait(false);
        if (fileResult == null)
        {
            return Error.NotFound("Image not found.");
        }
        var (fileBytes, contentType) = fileResult.Value;

        // If the file is not an image (e.g. PDF, DOCX, etc.), do not process or compress it.
        // Return the raw stream and correct Content-Type immediately.
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            var rawStream = new MemoryStream(fileBytes);
            return new ViewImageResponse { FileStream = rawStream, ContentType = contentType };
        }

        var InventoryReceiptStream = new MemoryStream(fileBytes);
        try
        {
            var processedStream = await fileReadService.ReadImageAsync(
                InventoryReceiptStream,
                request.Width,
                cancellationToken)
                .ConfigureAwait(false);
            return new ViewImageResponse { FileStream = processedStream, ContentType = "image/webp" };
        }
        catch (Exception)
        {
            // Fallback: If any exception occurs during image processing,
            // return the raw stream with the original content type.
            var rawStream = new MemoryStream(fileBytes);
            return new ViewImageResponse { FileStream = rawStream, ContentType = contentType };
        }
    }
}
