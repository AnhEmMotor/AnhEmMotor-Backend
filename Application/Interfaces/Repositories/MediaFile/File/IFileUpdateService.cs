
namespace Application.Interfaces.Repositories.MediaFile.File;

public interface IFileUpdateService
{
    public Task<Stream> CompressImageAsync(
        Stream inputStream,
        int quality,
        int? maxWidth,
        CancellationToken cancellationToken);
}
