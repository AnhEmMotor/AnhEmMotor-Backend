using Domain.Common.Models;
using MediatR;

namespace Application.Features.Files.Queries.GetFileById;

public sealed record GetFileByIdQuery(int Id) : IRequest<(ApiContracts.File.Responses.MediaFileResponse? Data, Common.Models.ErrorResponse? Error)>;
