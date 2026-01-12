using Application.ApiContracts.File.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Files.Queries.GetDeletedFilesList;

public sealed record GetDeletedFilesListQuery : IRequest<Result<PagedResult<MediaFileResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
