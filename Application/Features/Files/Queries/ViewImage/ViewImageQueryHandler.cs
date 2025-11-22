using Application.Interfaces.Repositories.File;
using Domain.Helpers;
using MediatR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.Features.Files.Queries.ViewImage;

public sealed class ViewImageQueryHandler(IMediaFileSelectRepository mediaFileSelectRepository, IFileRepository fileRepository)
    : IRequestHandler<ViewImageQuery, ((Stream fileStream, string contentType)? Data, ErrorResponse? Error)>
{
    public async Task<((Stream fileStream, string contentType)? Data, ErrorResponse? Error)> Handle(ViewImageQuery request, CancellationToken cancellationToken)
    {
        var media = await mediaFileSelectRepository.GetByStoredFileNameAsync(request.FileName, cancellationToken).ConfigureAwait(false);
        if (media is null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "File not found." }] });
        }

        var originalRelativePath = Path.Combine("uploads", request.FileName);
        if (!fileRepository.FileExists(originalRelativePath))
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "File not found." }] });
        }

        if (request.Width == null)
        {
            var originalStream = await fileRepository.ReadFileAsync(originalRelativePath, cancellationToken).ConfigureAwait(false);
            return ((originalStream!, "image/webp"), null);
        }

        try
        {
            using var originalStream = await fileRepository.ReadFileAsync(originalRelativePath, cancellationToken).ConfigureAwait(false);
            if (originalStream == null)
            {
                return (null, null);
            }

            using var image = await Image.LoadAsync(originalStream, cancellationToken).ConfigureAwait(false);

            var newWidth = request.Width.Value;
            var newHeight = (int)((double)image.Height / image.Width * newWidth);

            image.Mutate(x => x.Resize(newWidth, newHeight));

            var outputStream = new MemoryStream();
            await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = 75 }, cancellationToken).ConfigureAwait(false);

            outputStream.Position = 0;
            return ((outputStream, "image/webp"), null);
        }
        catch (Exception)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Image processing failed." }] });
        }
        
    }
}
