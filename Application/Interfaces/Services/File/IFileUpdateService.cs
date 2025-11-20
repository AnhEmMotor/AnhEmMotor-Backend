using Application.ApiContracts.File;
using Domain.Helpers;

namespace Application.Interfaces.Services.File
{
    public interface IFileUpdateService
    {
        Task<(FileResponse? Data, ErrorResponse? Error)> RestoreFileAsync(string fileName, CancellationToken cancellationToken);
        Task<(List<string>? Data, ErrorResponse? Error)> RestoreMultipleFilesAsync(List<string> fileNames, CancellationToken cancellationToken);
    }
}
