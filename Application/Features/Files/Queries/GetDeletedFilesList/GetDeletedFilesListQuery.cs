using Application.ApiContracts.File.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Files.Queries.GetDeletedFilesList;

public sealed record GetDeletedFilesListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<MediaFileResponse>>;
