using Domain.Helpers;

namespace Application.Interfaces.Services.File
{
    public interface IFileDeleteService
    {
        Task<ErrorResponse?> DeleteFileAsync(string fileName, CancellationToken cancellationToken);
        Task<ErrorResponse?> DeleteMultipleFilesAsync(List<string?> fileNames, CancellationToken cancellationToken);
    }
}
