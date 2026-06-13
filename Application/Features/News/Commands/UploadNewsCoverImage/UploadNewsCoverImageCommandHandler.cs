using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using MediatR;

namespace Application.Features.News.Commands.UploadNewsCoverImage;

public class UploadNewsCoverImageCommandHandler(IFileInsertService fileInsertService) : IRequestHandler<UploadNewsCoverImageCommand, Result<UploadNewsCoverImageResponse>>
{
    public async Task<Result<UploadNewsCoverImageResponse>> Handle(
        UploadNewsCoverImageCommand request,
        CancellationToken cancellationToken)
    {
        var result = await fileInsertService.SaveFileAsync(request.FileStream, cancellationToken, "articles/covers")
            .ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result<UploadNewsCoverImageResponse>.Failure(result.Errors);
        }
        var publicPath = $"/{result.Value.StoragePath}";
        var fullUrl = string.IsNullOrWhiteSpace(request.BaseUrl) ? publicPath : $"{request.BaseUrl}{publicPath}";
        return Result<UploadNewsCoverImageResponse>.Success(new UploadNewsCoverImageResponse { Url = fullUrl });
    }
}
