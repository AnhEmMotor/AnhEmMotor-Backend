
using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Queries.ViewImage;

public sealed record ViewImageQuery : IRequest<Result<ViewImageResponse?>>
{
    public string StoragePath { get; init; } = string.Empty;

    public int? Width { get; init; }
}
