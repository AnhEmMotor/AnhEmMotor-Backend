using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Queries.ViewImage;

public sealed record ViewImageQuery : IRequest<((Stream FileStream, string ContentType)? Data, ErrorResponse? Error)>
{
    public string StoragePath { get; init; } = string.Empty;

    public int? Width { get; init; }
}
