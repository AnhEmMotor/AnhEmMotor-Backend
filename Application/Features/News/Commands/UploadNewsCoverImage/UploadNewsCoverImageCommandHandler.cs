using Application.Common.Models;
using Application.Interfaces.Repositories.MediaFile.File;
using MediatR;

namespace Application.Features.News.Commands.UploadNewsCoverImage;

public sealed class UploadNewsCoverImageCommandHandler : IRequestHandler<UploadNewsCoverImageCommand, Result<UploadNewsCoverImageResponse>>
{
    private readonly IFileInsertService _fileInsertService;

    public UploadNewsCoverImageCommandHandler(IFileInsertService fileInsertService)
    {
        _fileInsertService = fileInsertService;
    }

    public async Task<Result<UploadNewsCoverImageResponse>> Handle(UploadNewsCoverImageCommand request, CancellationToken cancellationToken)
    {
        var result = await _fileInsertService.SaveFileAsync(request.FileStream, cancellationToken, "articles/covers").ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result<UploadNewsCoverImageResponse>.Failure(result.Errors);
        }

        var publicPath = $"/{result.Value.StoragePath}";
        var fullUrl = string.IsNullOrWhiteSpace(request.BaseUrl) ? publicPath : $"{request.BaseUrl}{publicPath}";
        
        return Result<UploadNewsCoverImageResponse>.Success(new UploadNewsCoverImageResponse { Url = fullUrl });
    }
}
