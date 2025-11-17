using Domain.Helpers;

namespace Application.Interfaces.Services.File
{
    public interface IFileSelectService
    {
        Task<((Stream fileStream, string contentType)? Data, ErrorResponse? Error)> GetImageAsync(string fileName, int? width, CancellationToken cancellationToken);
    }
}
