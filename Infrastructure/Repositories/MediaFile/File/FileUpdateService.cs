using Application.Interfaces.Repositories.MediaFile.File;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Repositories.MediaFile.File;

public class FileUpdateService : IFileUpdateService
{
    private const int DefaultMaxWidth = 1200;

    public async Task<Stream> CompressImageAsync(
        Stream InventoryReceiptStream,
        int quality,
        int? maxWidth,
        CancellationToken cancellationToken)
    {
        if (InventoryReceiptStream.CanSeek)
            InventoryReceiptStream.Position = 0;
        using var image = await Image.LoadAsync(InventoryReceiptStream, cancellationToken).ConfigureAwait(false);
        var targetWidth = maxWidth ?? DefaultMaxWidth;
        if (image.Width > targetWidth)
        {
            var newHeight = (int)((double)targetWidth / image.Width * image.Height);
            image.Mutate(x => x.Resize(targetWidth, newHeight));
        }
        var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = quality }, cancellationToken)
            .ConfigureAwait(false);
        outputStream.Position = 0;
        return outputStream;
    }
}
