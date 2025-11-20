using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.DeleteMultipleFiles;

public sealed record DeleteMultipleFilesCommand(List<string?> FileNames) : IRequest<ErrorResponse?>;
