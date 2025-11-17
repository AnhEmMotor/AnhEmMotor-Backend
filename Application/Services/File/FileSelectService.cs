using Application.Interfaces.Repositories.File;
using Application.Interfaces.Services.File;
using Domain.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.Services.File
{
    public class FileSelectService(IFileRepository fileRepository) : IFileSelectService
    {
        public async Task<((Stream fileStream, string contentType)? Data, ErrorResponse? Error)> GetImageAsync(string fileName, int? width, CancellationToken cancellationToken)
        {
            var originalRelativePath = Path.Combine("uploads", fileName);
            if (!fileRepository.FileExists(originalRelativePath))
            {
                return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "File not found." }] });
            }

            if (width == null)
            {
                var originalStream = await fileRepository.ReadFileAsync(originalRelativePath, cancellationToken).ConfigureAwait(false);
                return ((originalStream!, "image/webp"), null);
            }

            try
            {
                using var originalStream = await fileRepository.ReadFileAsync(originalRelativePath, cancellationToken).ConfigureAwait(false);
                if (originalStream == null) return (null, null);

                using var image = await Image.LoadAsync(originalStream, cancellationToken).ConfigureAwait(false);

                var newWidth = width.Value;
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
}
