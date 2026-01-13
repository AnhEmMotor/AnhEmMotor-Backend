using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Commands.UploadManyImage;

public sealed record UploadManyImageCommand : IRequest<Result<List<MediaFileResponse>>>
{
    public List<(Stream FileContent, string FileName)> Files { get; init; } = [];
}
