using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Files.Queries.GetFilesList;

public sealed record GetFilesListQuery : IRequest<Result<PagedResult<MediaFileResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
