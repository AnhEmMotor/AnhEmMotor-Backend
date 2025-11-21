using Application.ApiContracts.File;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreFile;

public record RestoreFileCommand(string FileName) : IRequest<(FileResponse?, ErrorResponse?)>;
