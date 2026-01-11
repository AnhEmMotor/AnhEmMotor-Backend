using Application.ApiContracts.File.Responses;
using MediatR;
using Sieve.Models;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Files.Queries.GetFilesList;

public sealed record GetFilesListQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<MediaFileResponse>>>;
