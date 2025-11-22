using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Queries.ViewImage;

public record ViewImageQuery(string FileName, int? Width) : IRequest<((Stream fileStream, string contentType)? Data, ErrorResponse? Error)>;
