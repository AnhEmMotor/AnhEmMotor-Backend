using Application.ApiContracts.File.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Files.Queries.GetFileById;

public sealed record GetFileByIdQuery(int Id) : IRequest<Result<MediaFileResponse?>>;
