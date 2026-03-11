using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Commands.UploadManyProductImages;

public sealed record UploadManyProductImagesCommand : IRequest<Result<List<MediaFileResponse>>>
{
    public List<(Stream FileContent, string FileName)> Files { get; init; } = [];
}
