using Application.ApiContracts.File.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.Files.Queries.GetFilesList;

public sealed record GetFilesListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<MediaFileResponse>>;
