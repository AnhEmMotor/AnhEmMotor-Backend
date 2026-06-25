using Application.ApiContracts.News.Responses;
using Application.Interfaces.Repositories.MediaFile.File;
using MediatR;

using Microsoft.AspNetCore.Http;

namespace Application.Features.News.Commands.UploadNewsContentImage;

public class UploadNewsContentImageCommandHandler(
    IFileInsertService fileInsertService,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<UploadNewsContentImageCommand, UploadNewsContentImageResponse>
{
    public async Task<UploadNewsContentImageResponse> Handle(
        UploadNewsContentImageCommand request,
        CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return new UploadNewsContentImageResponse { Errno = 1, Message = "File không hợp lệ." };
        }
        using var stream = request.File.OpenReadStream();
        var result = await fileInsertService.SaveFileAsync(stream, cancellationToken, "articles/content")
            .ConfigureAwait(false);
        if (result.IsFailure)
        {
            return new UploadNewsContentImageResponse { Errno = 1, Message = result.Error?.Message ?? "Upload failed" };
        }
        var publicPath = $"/{result.Value.StoragePath}";
        var requestObj = httpContextAccessor.HttpContext?.Request;
        var baseUrl = requestObj != null
            ? $"{requestObj.Scheme}://{requestObj.Host}{requestObj.PathBase}"
            : string.Empty;
        var fullUrl = string.IsNullOrWhiteSpace(baseUrl) ? publicPath : $"{baseUrl}{publicPath}";
        return new UploadNewsContentImageResponse
        {
            Errno = 0,
            Data = new UploadNewsContentImageData { Url = fullUrl, Alt = request.File.FileName, Href = fullUrl }
        };
    }
}
