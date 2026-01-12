using MediatR;
using Application.ApiContracts.File.Responses;
using Application.ApiContracts.File.Requests;
using Application.Common.Models;

namespace Application.Features.Files.Commands.UploadManyImage;

public sealed record UploadManyImageCommand : IRequest<Result<List<MediaFileResponse>>>
{
    public List<(Stream FileContent, string FileName)> Files { get; init; } = [];
}