using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Files.Queries.GetDeletedFilesList;

public sealed record GetDeletedFilesListQuery : IRequest<Result<PagedResult<MediaFileResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
