using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Files.Queries.GetDeletedFilesList;

public sealed record GetDeletedFilesListQuery(SieveModel SieveModel) : IRequest<PagedResult<ApiContracts.File.Responses.MediaFileResponse>>;
