using Application.ApiContracts.News.Responses;
using Application.Interfaces.Repositories.MediaFile.File;
using MediatR;

namespace Application.Features.News.Commands.UploadNewsContentImage;

public sealed class UploadNewsContentImageCommandHandler(IFileInsertService fileInsertService) : IRequestHandler<UploadNewsContentImageCommand, UploadNewsContentImageResponse>
{
    public async Task<UploadNewsContentImageResponse> Handle(
        UploadNewsContentImageCommand request,
        CancellationToken cancellationToken)
    {
        var result = await fileInsertService.SaveFileAsync(request.FileStream, cancellationToken, "articles/content")
            .ConfigureAwait(false);
        if (result.IsFailure)
        {
            return new UploadNewsContentImageResponse { Errno = 1, Message = result.Error?.Message ?? "Upload failed" };
        }
        var publicPath = $"/{result.Value.StoragePath}";
        var fullUrl = string.IsNullOrWhiteSpace(request.BaseUrl) ? publicPath : $"{request.BaseUrl}{publicPath}";
        return new UploadNewsContentImageResponse
        {
            Errno = 0,
            Data = new UploadNewsContentImageData { Url = fullUrl, Alt = request.FileName, Href = fullUrl }
        };
    }
}
