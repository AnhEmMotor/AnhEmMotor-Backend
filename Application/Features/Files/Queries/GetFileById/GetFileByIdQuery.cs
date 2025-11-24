using Application.ApiContracts.File;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Queries.GetFileById;

public sealed record GetFileByIdQuery(int Id) : IRequest<(MediaFileResponse? Data, ErrorResponse? Error)>;
