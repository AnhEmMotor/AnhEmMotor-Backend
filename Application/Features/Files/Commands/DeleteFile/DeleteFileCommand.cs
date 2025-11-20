using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.DeleteFile;

public sealed record DeleteFileCommand(string FileName) : IRequest<ErrorResponse?>;
