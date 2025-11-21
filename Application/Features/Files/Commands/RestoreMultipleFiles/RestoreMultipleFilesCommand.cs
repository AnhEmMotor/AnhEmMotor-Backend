using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreMultipleFiles;

public sealed record RestoreMultipleFilesCommand(List<string> FileNames) : IRequest<ErrorResponse?>;
