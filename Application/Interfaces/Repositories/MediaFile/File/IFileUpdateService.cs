
namespace Application.Interfaces.Repositories.MediaFile.File;

public interface IFileUpdateService
{
    public Task<Stream> CompressImageAsync(
        Stream InventoryReceiptStream,
        int quality,
        int? maxWidth,
        CancellationToken cancellationToken);
}
