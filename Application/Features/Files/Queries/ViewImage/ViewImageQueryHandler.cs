using Application.Interfaces.Services;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Queries.ViewImage;

public sealed class ViewImageQueryHandler(IFileStorageService fileStorageService)
    : IRequestHandler<ViewImageQuery, ((Stream FileStream, string ContentType)? Data, ErrorResponse? Error)>
{
    public async Task<((Stream FileStream, string ContentType)? Data, ErrorResponse? Error)> Handle(ViewImageQuery request, CancellationToken cancellationToken)
    {
        if (request.Width > 1200)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Width exceeds maximum allowed size of 1200 pixels." }] });
        }

        var fileResult = await fileStorageService.GetFileAsync(request.StoragePath, cancellationToken).ConfigureAwait(false);
        if (fileResult == null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Image not found." }] });
        }

        var (fileBytes, _) = fileResult.Value;

        try
        {
            using var inputStream = new MemoryStream(fileBytes);
            var processedStream = await fileStorageService.ReadImageAsync(inputStream, request.Width, cancellationToken).ConfigureAwait(false);

            return ((processedStream, "image/webp"), null);
        }
        catch (Exception ex)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = $"Image processing failed: {ex.Message}" }] });
        }
    }
}
