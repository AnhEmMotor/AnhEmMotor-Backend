using Application.Common.Models;

namespace Application.Interfaces.Repositories.MediaFile.File;

public interface IFileInsertService
{
    public Task<Result<FileUpload>> SaveFileAsync(
        Stream file,
        CancellationToken cancellationToken,
        string subFolder = "");
}

