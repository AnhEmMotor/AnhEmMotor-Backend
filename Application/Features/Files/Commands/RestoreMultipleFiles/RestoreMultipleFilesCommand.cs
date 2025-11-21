using Application.ApiContracts.File;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Files.Commands.RestoreMultipleFiles;

public record RestoreMultipleFilesCommand(List<string> FileNames) : IRequest<(List<FileResponse>?, ErrorResponse?)>;
