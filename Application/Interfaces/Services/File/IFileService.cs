using Application.ApiContracts.File;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.File
{
    public interface IFileService
    {
        Task<UploadResponse> UploadSingleFileAsync(IFormFile file, string baseUrl, CancellationToken cancellationToken);
        Task<List<UploadResponse>> UploadMultipleFilesAsync(List<IFormFile> files, string baseUrl, CancellationToken cancellationToken);
        Task<(Stream fileStream, string contentType)?> GetImageAsync(string fileName, int? width, CancellationToken cancellationToken);
    }
}
