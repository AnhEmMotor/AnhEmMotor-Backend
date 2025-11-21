using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreFile;

public sealed record RestoreFileCommand(string FileName) : IRequest<ErrorResponse?>;
