using Application.ApiContracts.File;
using Domain.Helpers;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.File
{
    public interface IFileInsertService
    {
        Task<(UploadResponse? Data, ErrorResponse? Error)> UploadSingleFileAsync(IFormFile file, string baseUrl, CancellationToken cancellationToken);
        Task<(List<UploadResponse>? Data, ErrorResponse? Error)> UploadMultipleFilesAsync(List<IFormFile> files, string baseUrl, CancellationToken cancellationToken);
    }
}
