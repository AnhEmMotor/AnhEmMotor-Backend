using Application.Interfaces.Services;
using Domain.Helpers;
using MediatR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.Features.Files.Queries.ViewImage;

public sealed class ViewImageQueryHandler(IFileStorageService fileStorageService)
    : IRequestHandler<ViewImageQuery, ((Stream FileStream, string ContentType)? Data, ErrorResponse? Error)>
{
    public async Task<((Stream FileStream, string ContentType)? Data, ErrorResponse? Error)> Handle(ViewImageQuery request, CancellationToken cancellationToken)
    {
        var maxWidth = 1200;

        if (request.Width > maxWidth)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = $"Width exceeds maximum allowed size of {maxWidth} pixels." }] });
        }

        var fileResult = await fileStorageService.GetFileAsync(request.StoragePath, cancellationToken).ConfigureAwait(false);
        if (fileResult == null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Image not found." }] });
        }

        var (fileBytes, contentType) = fileResult.Value;

        try
        {
            using var inputStream = new MemoryStream(fileBytes);
            using var image = await Image.LoadAsync(inputStream, cancellationToken).ConfigureAwait(false);

            var targetWidth = request.Width ?? maxWidth;

            if (image.Width > targetWidth)
            {
                var newHeight = (int)((double)targetWidth / image.Width * image.Height);
                image.Mutate(x => x.Resize(targetWidth, newHeight));
            }

            var outputStream = new MemoryStream();
            await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = 75 }, cancellationToken).ConfigureAwait(false);

            outputStream.Position = 0;
            return ((outputStream, "image/webp"), null);
        }
        catch (Exception ex)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = $"Image processing failed: {ex.Message}" }] });
        }
    }
}
