using Application.ApiContracts.File;
using Domain.Helpers;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.File
{
    public interface IFileInsertService
    {
        Task<(FileResponse? Data, ErrorResponse? Error)> UploadSingleFileAsync(IFormFile file, string baseUrl, CancellationToken cancellationToken);
        Task<(List<FileResponse>? Data, ErrorResponse? Error)> UploadMultipleFilesAsync(List<IFormFile> files, string baseUrl, CancellationToken cancellationToken);
    }
}
