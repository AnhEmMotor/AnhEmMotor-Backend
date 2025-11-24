using Application.ApiContracts.File;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Files.Queries.GetFilesList;

public sealed record GetFilesListQuery(SieveModel SieveModel) : IRequest<PagedResult<MediaFileResponse>>;
