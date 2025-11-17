using Domain.Helpers;

namespace Application.Interfaces.Services.File
{
    public interface IFileUpdateService
    {
        Task<ErrorResponse?> RestoreFileAsync(string fileName, CancellationToken cancellationToken);
        Task<ErrorResponse?> RestoreMultipleFilesAsync(List<string> fileNames, CancellationToken cancellationToken);
    }
}
