using Application.ApiContracts.Products.Responses;
using Application.Interfaces.Repositories.MediaFile.File;
using MediatR;

using Microsoft.AspNetCore.Http;

namespace Application.Features.Products.Commands.UploadProductContentImage;

public class UploadProductContentImageCommandHandler(
    IFileInsertService fileInsertService,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<UploadProductContentImageCommand, UploadProductContentImageResponse>
{
    public async Task<UploadProductContentImageResponse> Handle(
        UploadProductContentImageCommand request,
        CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return new UploadProductContentImageResponse { Errno = 1, Message = "File không hợp lệ." };
        }
        using var stream = request.File.OpenReadStream();
        var result = await fileInsertService.SaveFileAsync(stream, cancellationToken, "products/content")
            .ConfigureAwait(false);
        if (result.IsFailure)
        {
            return new UploadProductContentImageResponse
            {
                Errno = 1,
                Message = result.Error?.Message ?? "Upload failed"
            };
        }
        var publicPath = $"/{result.Value.StoragePath}";
        var requestObj = httpContextAccessor.HttpContext?.Request;
        var baseUrl = requestObj != null
            ? $"{requestObj.Scheme}://{requestObj.Host}{requestObj.PathBase}"
            : string.Empty;
        var fullUrl = string.IsNullOrWhiteSpace(baseUrl) ? publicPath : $"{baseUrl}{publicPath}";
        return new UploadProductContentImageResponse
        {
            Errno = 0,
            Data = new UploadProductContentImageData { Url = fullUrl, Alt = request.File.FileName, Href = fullUrl }
        };
    }
}
