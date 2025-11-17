using Application.ApiContracts.File;
using Domain.Helpers;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.File
{
    public interface IFileService
    {
        Task<(UploadResponse? Data, ErrorResponse? Error)> UploadSingleFileAsync(IFormFile file, string baseUrl, CancellationToken cancellationToken);
        Task<(List<UploadResponse>? Data, ErrorResponse? Error)> UploadMultipleFilesAsync(List<IFormFile> files, string baseUrl, CancellationToken cancellationToken);
        Task<((Stream fileStream, string contentType)? Data, ErrorResponse? Error)> GetImageAsync(string fileName, int? width, CancellationToken cancellationToken);
    }
}
