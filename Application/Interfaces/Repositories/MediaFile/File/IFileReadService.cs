
namespace Application.Interfaces.Repositories.MediaFile.File;

public interface IFileReadService
{
    public string GetPublicUrl(string storagePath);

    public Task<(byte[] FileBytes, string ContentType)?> GetFileAsync(
        string storagePath,
        CancellationToken cancellationToken);

    public Task<Stream> ReadImageAsync(Stream InventoryReceiptStream, int? width, CancellationToken cancellationToken);
}

