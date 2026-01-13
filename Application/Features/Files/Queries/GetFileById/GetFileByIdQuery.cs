using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Queries.GetFileById;

public sealed record GetFileByIdQuery : IRequest<Result<MediaFileResponse?>>
{
    public int? Id { get; init; }
}
